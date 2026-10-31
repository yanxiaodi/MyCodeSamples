"""The model requests an action; the host applies an official AGT decision."""
from __future__ import annotations

import json
import time
import warnings
from dataclasses import asdict, dataclass, field
from pathlib import Path
from uuid import uuid4

from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

with warnings.catch_warnings():
    warnings.filterwarnings("ignore", category=DeprecationWarning, message=r"agentmesh-platform is deprecated.*")
    from agentmesh.governance import PolicyEngine

from .providers import InvalidToolCall, Provider
from .tools import ToolRepository
from .telemetry import logger, provider_context, record_policy, record_tool, record_run

POLICY_PATH = Path(__file__).resolve().parents[1] / "policy.yaml"
SCENARIOS = {
    "allowed": {"prompt": "Look up customer C-1042 and summarize their support status.", "tool": "lookup_customer"},
    "blocked": {"prompt": "Delete customer record C-1042.", "tool": "delete_record"},
    "failed": {"prompt": "Look up customer C-1042 and summarize their support status.", "tool": "lookup_customer"},
}


@dataclass
class RunResult:
    run_id: str
    scenario: str
    provider: str
    outcome: str = "failed"
    policy_decision: str = "not_evaluated"
    rule: str = ""
    reason: str = ""
    answer: str = ""
    error_type: str = ""
    trace_id: str = ""
    duration_ms: float = 0
    input_tokens: int = 0
    output_tokens: int = 0
    tool_body_counts: dict = field(default_factory=lambda: {"lookup_customer": 0, "delete_record": 0})

    def to_dict(self):
        return asdict(self)


class AgentRunner:
    def __init__(self, provider: Provider, *, timeout_seconds=0.8):
        self.provider, self.timeout_seconds = provider, timeout_seconds

    def run(self, scenario: str) -> RunResult:
        if scenario not in SCENARIOS:
            raise ValueError("Choose allowed, blocked, or failed.")
        spec = SCENARIOS[scenario]
        tools = ToolRepository(simulate_timeout=scenario == "failed", timeout_seconds=self.timeout_seconds)
        result = RunResult(str(uuid4()), scenario, self.provider.mode)
        engine = PolicyEngine()
        engine.load_yaml_file(str(POLICY_PATH))
        tracer = trace.get_tracer("support-agent")
        token = provider_context.set(self.provider.mode)
        started = time.perf_counter()
        with tracer.start_as_current_span("invoke_agent support-agent", attributes={
            "gen_ai.operation.name": "invoke_agent", "gen_ai.agent.name": "support-agent",
            "app.demo.scenario": scenario, "app.demo.provider": self.provider.mode,
            "app.run.id": result.run_id,
        }) as root:
            result.trace_id = format(root.get_span_context().trace_id, "032x")
            try:
                messages = [
                    {"role": "system", "content": "You are a support agent using synthetic customer records. Request one tool, then summarize its result. The host enforces all permissions."},
                    {"role": "user", "content": spec["prompt"]},
                ]
                turn = self.provider.complete(messages, {"type": "function", "function": {"name": spec["tool"]}})
                result.input_tokens += turn.input_tokens
                result.output_tokens += turn.output_tokens
                if len(turn.calls) != 1:
                    raise InvalidToolCall("The demo requires exactly one tool request.")
                call = turn.calls[0]
                self._validate(call.name, call.arguments)
                with tracer.start_as_current_span("policy.evaluate " + call.name, attributes={
                    "app.tool.name": call.name, "app.governance.policy": "support-agent-demo",
                    "app.governance.policy_version": "1.0",
                }) as policy_span:
                    decision = engine.evaluate("support-agent", {"action": {"type": call.name}, "args": call.arguments})
                    result.policy_decision = decision.action
                    result.rule, result.reason = decision.matched_rule or "", decision.reason or ""
                    policy_span.set_attribute("app.governance.decision", decision.action)
                    policy_span.set_attribute("app.governance.rule", result.rule)
                    record_policy(decision.action, call.name, result.rule)
                # Enforcement is explicit and precedes the tool body.
                if not decision.allowed:
                    result.outcome = "blocked"
                    result.answer = "Deletion was blocked by policy. The tool did not execute."
                    return result
                tool_started = time.perf_counter()
                with tracer.start_as_current_span("execute_tool " + call.name, attributes={
                    "gen_ai.operation.name": "execute_tool", "gen_ai.tool.name": call.name,
                }) as tool_span:
                    try:
                        output = getattr(tools, call.name)(**call.arguments)
                    except Exception as exc:
                        tool_span.set_status(Status(StatusCode.ERROR))
                        tool_span.set_attribute("error.type", type(exc).__name__)
                        record_tool(call.name, "failed", time.perf_counter() - tool_started)
                        raise
                    record_tool(call.name, "succeeded", time.perf_counter() - tool_started)
                messages.extend([
                    {"role": "assistant", "content": None, "tool_calls": [{"id": call.id, "type": "function", "function": {
                        "name": call.name, "arguments": json.dumps(call.arguments)}}]},
                    {"role": "tool", "tool_call_id": call.id, "content": json.dumps(output)},
                ])
                final = self.provider.complete(messages, "none")
                result.input_tokens += final.input_tokens
                result.output_tokens += final.output_tokens
                if final.calls or not final.text:
                    raise InvalidToolCall("The final model turn must return an answer without additional tools.")
                result.answer, result.outcome = final.text, "succeeded"
            except Exception as exc:
                result.outcome, result.error_type = "failed", type(exc).__name__
                result.answer = ("Lookup was allowed, but the customer service timed out." if isinstance(exc, TimeoutError)
                                 else "The run failed. Check its trace and configuration; no mock fallback was used.")
                root.set_status(Status(StatusCode.ERROR))
                root.set_attribute("error.type", result.error_type)
                logger.error("Agent run failed", extra={"error.type": result.error_type, "app.run.outcome": "failed"})
            finally:
                result.tool_body_counts = dict(tools.body_counts)
                result.duration_ms = round((time.perf_counter() - started) * 1000, 2)
                root.set_attribute("app.run.outcome", result.outcome)
                record_run(result.outcome, result.duration_ms / 1000)
                provider_context.reset(token)
        return result

    @staticmethod
    def _validate(name, arguments):
        if name not in {"lookup_customer", "delete_record"}:
            raise InvalidToolCall("The model requested an unknown tool.")
        if not isinstance(arguments, dict) or set(arguments) != {"customer_id"}:
            raise InvalidToolCall("Tool arguments must contain only customer_id.")
        if not isinstance(arguments["customer_id"], str) or not arguments["customer_id"].strip():
            raise InvalidToolCall("A non-empty customer_id is required.")
