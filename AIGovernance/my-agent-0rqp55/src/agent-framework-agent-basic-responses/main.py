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
    # When Azure Monitor owns the OTEL providers, opt in to sensitive-data
    # capture so prompts and completions appear in traces.
    from agent_framework.observability import enable_sensitive_telemetry

    enable_sensitive_telemetry()
else:
    # No Azure Monitor – let the Agent Framework SDK configure OpenTelemetry
    # tracing. When running locally with Foundry Toolkit, traces are sent to
    # localhost:4317 (gRPC) / localhost:4318 (HTTP). In production the standard
    # OTEL_EXPORTER_OTLP_ENDPOINT variable controls the destination.
    from agent_framework.observability import configure_otel_providers

    configure_otel_providers(
        vs_code_extension_port=4317,
        enable_sensitive_data=True,
    )

from agent_framework import (
    Agent,
    AgentContext,
    AgentMiddleware,
    AgentResponse,
    FunctionInvocationContext,
    FunctionMiddleware,
    Message,
    MiddlewareTermination,
    tool,
)
from agent_framework.foundry import FoundryChatClient
from agent_framework_foundry_hosting import ResponsesHostServer
from agent_control_specification import (
    AgentControl,
    EnforcementMode,
    InterventionPoint,
)
from azure.identity import DefaultAzureCredential

from tools import delete_record, lookup_customer_messages, send_email

SOURCE_DIR = Path(__file__).resolve().parent
POLICY_MANIFEST = SOURCE_DIR / "policies" / "manifest.yaml"
logger = logging.getLogger("agent_governance_demo")
logger.setLevel(logging.INFO)


def _last_message_text(context: AgentContext) -> str:
    """Return the most recent message text from an Agent Framework context."""

    messages: list[Any] = getattr(context, "messages", None) or []
    if not messages:
        return ""
    last_message = messages[-1]
    return getattr(last_message, "text", None) or str(last_message)


def _policy_reason(result: Any, error: Exception) -> str:
    """Extract a useful governance reason for a user-visible denial."""

    verdict = getattr(result, "verdict", None)
    reason = getattr(verdict, "reason", None)
    return str(reason or getattr(result, "reason", None) or error or "policy_denied")


class _AgentControlInputMiddleware(AgentMiddleware):
    """Apply ACS input governance using the published AgentControl API."""

    def __init__(self, control: AgentControl) -> None:
        self.control = control

    async def process(
        self,
        context: AgentContext,
        call_next: Any,
    ) -> None:
        result = await self.control.evaluate_intervention_point(
            InterventionPoint.INPUT,
            {"input": {"body": _last_message_text(context)}},
        )
        try:
            await self.control.enforce(
                InterventionPoint.INPUT,
                result,
                EnforcementMode.ENFORCE,
            )
        except Exception as exc:
            reason = _policy_reason(result, exc)
            context.result = AgentResponse(
                messages=[Message("assistant", [f"⛔ Policy violation: {reason}"])]
            )
            raise MiddlewareTermination(reason) from exc

        await call_next()


class _AgentControlToolMiddleware(FunctionMiddleware):
    """Apply ACS pre-tool governance using the published AgentControl API."""

    def __init__(self, control: AgentControl) -> None:
        self.control = control

    async def process(
        self,
        context: FunctionInvocationContext,
        call_next: Any,
    ) -> None:
        function = getattr(context, "function", None)
        tool_name = getattr(function, "name", "unknown")
        raw_arguments = getattr(context, "arguments", None)
        if isinstance(raw_arguments, dict):
            arguments = dict(raw_arguments)
        elif raw_arguments is None:
            arguments = {}
        else:
            arguments = {"_value": raw_arguments}

        result = await self.control.evaluate_intervention_point(
            InterventionPoint.PRE_TOOL_CALL,
            {"tool_call": {"name": tool_name, "args": arguments}},
        )
        try:
            await self.control.enforce(
                InterventionPoint.PRE_TOOL_CALL,
                result,
                EnforcementMode.ENFORCE,
            )
        except Exception as exc:
            reason = _policy_reason(result, exc)
            context.result = (
                f"⛔ Tool '{tool_name}' is not permitted by governance policy"
            )
            raise MiddlewareTermination(reason) from exc

        if getattr(result, "transformed_policy_target_applied", False):
            context.arguments = result.transformed_policy_target

        await call_next()


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

    runtime = AgentControl.from_path(str(POLICY_MANIFEST))
    middleware = [
        _AgentControlInputMiddleware(runtime),
        _AgentControlToolMiddleware(runtime),
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
            close = getattr(runtime, "close", None)
            if callable(close):
                close()


if __name__ == "__main__":
    main()
