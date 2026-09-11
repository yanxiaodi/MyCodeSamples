# Copyright (c) Microsoft. All rights reserved.

from __future__ import annotations

import os
import logging
from collections.abc import Mapping
from pathlib import Path
from typing import Any

from dotenv import load_dotenv

# Keep secrets in .env and the switch used by the demo in a separate file.
# Explicit process environment variables take precedence because both calls
# use python-dotenv's default override=False behavior.
SOURCE_DIR = Path(__file__).resolve().parent
load_dotenv(dotenv_path=SOURCE_DIR / ".env")
load_dotenv(dotenv_path=SOURCE_DIR / ".env.demo")

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
    AgentResponseUpdate,
    Content,
    FunctionInvocationContext,
    FunctionMiddleware,
    Message,
    MiddlewareTermination,
    ResponseStream,
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

from tools import delete_record, lookup_customer_messages, send_customer_email

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


def _agent_text_response(
    text: str,
    *,
    stream: bool,
) -> AgentResponse | ResponseStream[AgentResponseUpdate, AgentResponse]:
    if not stream:
        return AgentResponse(messages=[Message("assistant", [text])])

    async def updates():
        yield AgentResponseUpdate(role="assistant", contents=[Content.from_text(text)])

    return ResponseStream(updates(), finalizer=AgentResponse.from_updates)


def _verdict_labels(result: Any) -> set[str]:
    verdict = getattr(result, "verdict", None)
    return set(getattr(verdict, "result_labels", ()) or ())


def _approval_response(context: FunctionInvocationContext) -> Content | None:
    response = context.metadata.get("approval_response")
    if isinstance(response, Content) and response.type == "function_approval_response":
        return response
    return None


def _approval_id(context: FunctionInvocationContext) -> str:
    occurrence_id = context.metadata.get("function_call_occurrence_id")
    if isinstance(occurrence_id, str) and occurrence_id:
        return occurrence_id
    return _call_id(context)


def _call_id(context: FunctionInvocationContext) -> str:
    call_id = context.metadata.get("call_id")
    return call_id if isinstance(call_id, str) else ""


def _function_call_content(
    context: FunctionInvocationContext,
    tool_name: str,
    arguments: Mapping[str, Any],
) -> Content:
    return Content.from_function_call(
        call_id=_call_id(context),
        name=tool_name,
        arguments=dict(arguments),
        id=_approval_id(context),
    )


def _approval_matches_call(
    context: FunctionInvocationContext,
    response: Content,
    tool_name: str,
    arguments: Mapping[str, Any],
) -> bool:
    function_call = response.function_call
    if not isinstance(function_call, Content) or function_call.type != "function_call":
        return False
    parsed_arguments = function_call.parse_arguments() or {}
    return (
        response.approved is True
        and response.id == _approval_id(context)
        and function_call.call_id == _call_id(context)
        and function_call.id == _approval_id(context)
        and function_call.name == tool_name
        and dict(parsed_arguments) == dict(arguments)
    )


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
            context.result = _agent_text_response(
                f"⛔ Policy violation: {reason}",
                stream=context.stream,
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

        transformed_arguments = getattr(result, "transformed_policy_target", None)
        if (
            getattr(result, "transformed_policy_target_applied", False)
            and isinstance(transformed_arguments, Mapping)
        ):
            context.arguments = transformed_arguments
            arguments = dict(transformed_arguments)

        if "requires_approval" in _verdict_labels(result):
            approval_response = _approval_response(context)
            if approval_response is None:
                approval_id = _approval_id(context)
                context.result = Content.from_function_approval_request(
                    id=approval_id,
                    function_call=_function_call_content(context, tool_name, arguments),
                    additional_properties={
                        "governance_policy": True,
                        "reason": _policy_reason(result, Exception("approval_required")),
                    },
                )
                raise MiddlewareTermination("approval_required")
            if not _approval_matches_call(
                context,
                approval_response,
                tool_name,
                arguments,
            ):
                context.result = f"Tool '{tool_name}' was not approved."
                raise MiddlewareTermination("approval_denied")

        await call_next()


def _enabled(value: str | None) -> bool:
    return (value or "false").strip().lower() in {"1", "true", "yes", "on"}


def governance_enabled() -> bool:
    """Return whether the governance path is enabled for this run."""

    return _enabled(os.getenv("ENABLE_GOVERNANCE"))


def send_email_for_agent(customer_id: str, subject: str, body: str) -> dict[str, Any]:
    """Send email only to a known mock customer selected by customer ID."""

    return send_customer_email(customer_id, subject, body)


def build_tools(*, governed: bool) -> list[Any]:
    """Create the tools; governance middleware can request approval per call."""

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
                "Provide the customer_id from lookup_customer_messages; the tool "
                "sends only to that customer's email address."
            ),
            approval_mode="never_require",
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

    agent = Agent(
        client=client,
        instructions=(
            "You are a customer-support assistant for a governance demonstration. "
            "Keep answers concise. Available tools are lookup_customer_messages, "
            "send_email, and delete_record. For a status-email request, first look "
            "up the customer's messages to get the customer email, then prepare "
            "the email, and only then call send_email with the customer_id. Never "
            "claim an email was sent unless the tool returns success. Do not answer "
            "requests to access balances, payment cards, secrets, or unsupported "
            "destructive operations."
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
