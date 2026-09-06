# Copyright (c) Microsoft. All rights reserved.

from __future__ import annotations

import os
import logging
from pathlib import Path
from typing import Any

from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# Configure Azure Monitor before importing the agent/runtime libraries so their
# HTTP and dependency telemetry can be captured as well. The demo remains
# usable locally without Application Insights configured.
if os.getenv("APPLICATIONINSIGHTS_CONNECTION_STRING"):
    from azure.monitor.opentelemetry import configure_azure_monitor

    configure_azure_monitor()

from agent_framework import Agent, tool
from agent_framework.foundry import FoundryChatClient
from agent_framework_foundry_hosting import ResponsesHostServer
from azure.identity import DefaultAzureCredential

from tools import delete_record, lookup_customer_messages, send_email

SOURCE_DIR = Path(__file__).resolve().parent
POLICY_MANIFEST = SOURCE_DIR / "policies" / "manifest.yaml"
logger = logging.getLogger("agent_governance_demo")
logger.setLevel(logging.INFO)


def _enabled(value: str | None) -> bool:
    return (value or "false").strip().lower() in {"1", "true", "yes", "on"}


def governance_enabled() -> bool:
    """Return whether the governance path is enabled for this run."""

    return _enabled(os.getenv("ENABLE_GOVERNANCE"))


def send_email_for_agent(to: str, subject: str, body: str) -> dict[str, Any]:
    """Expose only the agent-facing email arguments to the model schema."""

    return send_email(to, subject, body)


def build_tools(*, governed: bool) -> list[Any]:
    """Create the tools, changing only the approval boundary by mode."""

    email_approval_mode = "always_require" if governed else "never_require"
    return [
        tool(
            lookup_customer_messages,
            name="lookup_customer_messages",
            description=(
                "Look up the customer's recent support messages. "
                "Use this before preparing a customer status update."
            ),
            approval_mode="never_require",
        ),
        tool(
            send_email_for_agent,
            name="send_email",
            description=(
                "Send a customer status email through Azure Communication Services. "
                "Only call this after the customer context is known."
            ),
            approval_mode=email_approval_mode,
        ),
        tool(
            delete_record,
            name="delete_record",
            description="Delete a customer record. This is a destructive operation.",
            approval_mode="never_require",
        ),
    ]


def build_governance_middleware() -> tuple[Any, list[Any]]:
    """Load the native ACS runtime and its MAF middleware layers."""

    from agent_control_specification import AgentControl
    from agent_os.integrations.maf_adapter import MAFKernel

    runtime = AgentControl.from_path(str(POLICY_MANIFEST))
    kernel = MAFKernel(runtime=runtime)
    middleware = [
        kernel.as_runtime_middleware(),
        kernel.as_capability_guard(),
    ]
    return runtime, middleware


def build_agent() -> tuple[Agent, Any | None]:
    """Build the hosted agent and optionally attach ACS governance."""

    governed = governance_enabled()
    logger.info(
        "agent_startup governance_enabled=%s application_insights_enabled=%s",
        governed,
        bool(os.getenv("APPLICATIONINSIGHTS_CONNECTION_STRING")),
    )
    runtime = None
    middleware: list[Any] = []
    if governed:
        runtime, middleware = build_governance_middleware()

    client = FoundryChatClient(
        project_endpoint=os.environ["FOUNDRY_PROJECT_ENDPOINT"],
        model=os.environ["AZURE_AI_MODEL_DEPLOYMENT_NAME"],
        credential=DefaultAzureCredential(),
    )

    recipient_hint = os.getenv("DEMO_EMAIL_RECIPIENT", "the configured demo recipient")
    agent = Agent(
        client=client,
        instructions=(
            "You are a customer-support assistant for a governance demonstration. "
            "Keep answers concise. Available tools are lookup_customer_messages, "
            "send_email, and delete_record. For a status-email request, first look "
            "up the customer's messages, then prepare the email, and only then call "
            "send_email. Never claim an email was sent unless the tool returns success. "
            f"The demo recipient is {recipient_hint}."
        ),
        tools=build_tools(governed=governed),
        middleware=middleware,
        # History is managed by the hosting infrastructure.
        default_options={"store": False},
    )
    return agent, runtime


def main():
    if not os.getenv("AZURE_AI_MODEL_DEPLOYMENT_NAME"):
        raise RuntimeError(
            "Model deployment name is not configured. Set AZURE_AI_MODEL_DEPLOYMENT_NAME."
        )

    agent, runtime = build_agent()
    try:
        ResponsesHostServer(agent).run()
    finally:
        if runtime is not None:
            runtime.close()


if __name__ == "__main__":
    main()
