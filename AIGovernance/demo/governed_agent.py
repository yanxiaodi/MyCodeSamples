"""Agent Governance Toolkit demo for the meetup talk.

The model chooses a tool, but every tool is wrapped with the official
``agentmesh.governance.govern`` API before it can execute. The local mock
provider is the default so the demo is repeatable and has no network side
effects unless the approved email action is configured for ACS.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
import warnings
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Protocol

# The package-consolidation migration keeps this public module path stable.
# AGT 5.0 still emits its own compatibility warning while importing it; keep
# that upstream warning local to this documented wrapper import so the meetup
# demo does not bury its policy output in dependency noise.
with warnings.catch_warnings():
    warnings.filterwarnings(
        "ignore",
        category=DeprecationWarning,
        message=r"agentmesh-platform is deprecated.*agent-governance-toolkit-core.*",
    )
    from agentmesh.governance import (
        ApprovalDecision,
        CallbackApproval,
        GovernanceDenied,
        GovernedCallable,
        govern,
    )


DEMO_ROOT = Path(__file__).resolve().parent
POLICY_PATH = DEMO_ROOT / "policy.yaml"


def load_dotenv(path: Path) -> None:
    """Load simple KEY=VALUE settings without adding python-dotenv."""
    if not path.exists():
        return
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        line = re.sub(r"^export\s+", "", line)
        if "=" not in line:
            continue
        key, value = line.split("=", 1)
        key = key.strip()
        value = value.strip().strip("\"'")
        if key:
            os.environ.setdefault(key, value)


load_dotenv(DEMO_ROOT / ".env")


@dataclass(frozen=True)
class ToolCall:
    tool: str
    arguments: dict[str, Any]
    prompt: str


def normalize_tool_arguments(
    tool: str, arguments: dict[str, Any], prompt: str = ""
) -> dict[str, Any]:
    """Translate model vocabulary to the callable's explicit tool contract."""
    normalized = dict(arguments)
    if tool == "send_email" and "to" not in normalized and "recipient" in normalized:
        normalized["to"] = normalized.pop("recipient")
    if tool in {"lookup_customer", "delete_record"} and "customer_id" not in normalized:
        for alias in ("customerId", "id", "customer"):
            if alias in normalized:
                normalized["customer_id"] = normalized.pop(alias)
                break
        if "customer_id" not in normalized:
            requested_id = re.search(r"\bC-\d+\b", prompt, re.IGNORECASE)
            if requested_id:
                normalized["customer_id"] = requested_id.group(0).upper()
    return normalized


REQUIRED_TOOL_ARGUMENTS = {
    "lookup_customer": ("customer_id",),
    "send_email": ("to", "subject"),
    "delete_record": ("customer_id",),
}


class ModelProvider(Protocol):
    def choose_tool(self, prompt: str) -> ToolCall:
        ...


class MockProvider:
    """A predictable provider for rehearsals and offline demos."""

    def choose_tool(self, prompt: str) -> ToolCall:
        lowered = prompt.lower()
        if any(word in lowered for word in ("delete", "remove", "erase")):
            return ToolCall("delete_record", {"customer_id": "C-1042"}, prompt)
        if any(word in lowered for word in ("email", "send", "message")):
            return ToolCall(
                "send_email",
                {
                    "to": os.getenv("DEMO_EMAIL_RECIPIENT", "demo-recipient@example.net"),
                    "subject": "Support update",
                },
                prompt,
            )
        return ToolCall("lookup_customer", {"customer_id": "C-1042"}, prompt)


class FoundryProvider:
    """Azure AI Foundry provider using the OpenAI Responses API."""

    def __init__(self) -> None:
        endpoint = os.environ["AZURE_AI_FOUNDRY_ENDPOINT"].rstrip("/")
        if endpoint.endswith("/responses"):
            endpoint = endpoint[: -len("/responses")]
        self.endpoint = endpoint
        self.api_key = os.environ["AZURE_AI_FOUNDRY_API_KEY"]
        self.deployment = os.environ["AZURE_AI_FOUNDRY_DEPLOYMENT"]
        try:
            from openai import OpenAI
        except ImportError as error:
            raise RuntimeError(
                "Install demo requirements first: python -m pip install -r requirements.txt"
            ) from error
        self.client = OpenAI(base_url=self.endpoint, api_key=self.api_key)

    def choose_tool(self, prompt: str) -> ToolCall:
        response = self.client.responses.create(
            model=self.deployment,
            instructions=(
                "Choose exactly one tool. Return JSON only with keys tool and arguments. "
                "Allowed tools: lookup_customer, send_email, delete_record. "
                "For lookup_customer and delete_record, arguments must be "
                "{\"customer_id\": \"C-1042\"}. "
                "For send_email, arguments must be exactly "
                "{\"to\": \"email address\", \"subject\": \"短 subject\"}; "
                "use the key 'to', never 'recipient'."
            ),
            input=prompt,
        )
        content = getattr(response, "output_text", "")
        if not content:
            raise RuntimeError("Foundry Responses API returned no output text")
        content = re.sub(r"^```(?:json)?\s*|\s*```$", "", content.strip())
        selected = json.loads(content)
        return ToolCall(selected["tool"], selected.get("arguments", {}), prompt)


class FallbackProvider:
    """Use Foundry first, then keep a rehearsal running if it is unavailable."""

    def __init__(self, primary: ModelProvider, fallback: ModelProvider) -> None:
        self.primary = primary
        self.fallback = fallback
        self._used_fallback = False

    def choose_tool(self, prompt: str) -> ToolCall:
        if self._used_fallback:
            return self.fallback.choose_tool(prompt)
        try:
            return self.primary.choose_tool(prompt)
        except Exception as error:
            self._used_fallback = True
            print(
                f"Foundry unavailable ({type(error).__name__}); using local mock fallback.",
                file=sys.stderr,
            )
            return self.fallback.choose_tool(prompt)


def select_provider(name: str) -> tuple[ModelProvider, str]:
    configured = all(
        os.getenv(key)
        for key in (
            "AZURE_AI_FOUNDRY_ENDPOINT",
            "AZURE_AI_FOUNDRY_API_KEY",
            "AZURE_AI_FOUNDRY_DEPLOYMENT",
        )
    )
    if name == "foundry":
        try:
            return FoundryProvider(), "foundry"
        except KeyError as error:
            raise RuntimeError(f"Missing Foundry configuration: {error.args[0]}") from error
    if name == "auto" and configured:
        return FallbackProvider(FoundryProvider(), MockProvider()), "foundry (mock fallback)"
    return MockProvider(), "mock"


class DemoTools:
    """Local customer tools plus an optional real ACS email tool.

    These are the actual callables passed to ``govern()``. AGT evaluates the
    policy before their bodies run, so a denied delete never reaches its body.
    """

    def __init__(self) -> None:
        self._email_client: Any | None = None
        self._email_sender = os.getenv("AZURE_COMMUNICATION_SERVICES_SENDER", "")
        self._email_connection_string = os.getenv(
            "AZURE_COMMUNICATION_SERVICES_CONNECTION_STRING", ""
        )

    def lookup_customer(self, *, action: str, customer_id: str) -> dict[str, Any]:
        return {"customer_id": customer_id, "name": "Demo Customer", "status": "active"}

    def send_email(self, *, action: str, to: str, subject: str) -> dict[str, Any]:
        if not self._email_connection_string or not self._email_sender:
            return {"provider": "fake", "to": to, "message_id": "demo-message-001"}

        if self._email_client is None:
            try:
                from azure.communication.email import EmailClient
            except ImportError as error:
                raise RuntimeError(
                    "Install demo requirements first: python -m pip install -r requirements.txt"
                ) from error
            self._email_client = EmailClient.from_connection_string(self._email_connection_string)

        message = {
            "senderAddress": self._email_sender,
            "recipients": {"to": [{"address": to}]},
            "content": {
                "subject": subject,
                "plainText": "This is a governed customer support status update.",
            },
        }
        poller = self._email_client.begin_send(message)
        result = poller.result()
        message_id = result.get("id") if isinstance(result, dict) else getattr(result, "id", None)
        return {
            "provider": "azure-communication-services",
            "to": to,
            "message_id": message_id or "accepted",
        }

    def delete_record(self, *, action: str, customer_id: str) -> dict[str, Any]:
        raise AssertionError("delete_record must never execute in this demo")


class GovernedAgent:
    def __init__(self, provider: ModelProvider) -> None:
        self.provider = provider
        self.tools = DemoTools()
        self._approve_escalation: bool | None = None
        approval_handler = CallbackApproval(self._request_approval)
        self.governed_tools: dict[str, GovernedCallable] = {
            name: govern(
                getattr(self.tools, name),
                policy=str(POLICY_PATH),
                agent_id="customer-support-agent",
                session_id="demo-session-001",
                approval_handler=approval_handler,
            )
            for name in ("lookup_customer", "send_email", "delete_record")
        }

    def run(self, prompt: str, approve_escalation: bool | None = None) -> dict[str, Any]:
        call = self.provider.choose_tool(prompt)
        self._approve_escalation = approve_escalation
        result: dict[str, Any] = {"tool": call.tool, "arguments": call.arguments}
        governed_tool = self.governed_tools.get(call.tool)
        if governed_tool is None:
            result.update({"executed": False, "status": "blocked", "reason": "Unknown tool"})
            return result

        arguments = normalize_tool_arguments(call.tool, call.arguments, call.prompt)
        result["arguments"] = arguments
        missing = [
            name
            for name in REQUIRED_TOOL_ARGUMENTS.get(call.tool, ())
            if not arguments.get(name)
        ]
        if missing:
            result.update(
                {
                    "executed": False,
                    "status": "blocked",
                    "reason": f"Missing required tool argument(s): {', '.join(missing)}",
                }
            )
            return result
        arguments.pop("action", None)
        try:
            output = governed_tool(action=call.tool, **arguments)
        except GovernanceDenied as error:
            decision = error.decision
            reason = decision.reason or "Blocked by policy"
            result.update(
                {
                    "decision": {
                        "action": decision.action,
                        "reason": reason,
                        "matched_rule": decision.matched_rule,
                    },
                    "executed": False,
                    "status": "approval_required" if "Approval rejected" in reason else "blocked",
                }
            )
            return result

        result.update(
            {
                "decision": self._latest_decision(call.tool),
                "executed": True,
                "status": "executed",
                "output": output,
            }
        )
        return result

    def _request_approval(self, request: Any) -> ApprovalDecision:
        approved = self._approve_escalation
        if approved is None:
            print("\nAGT policy requires approval for send_email.")
            try:
                approved = input("Approve sending the email? [y/N]: ").strip().lower() in {
                    "y",
                    "yes",
                }
            except (EOFError, KeyboardInterrupt):
                print("\nApproval cancelled.")
                approved = False

        if approved:
            return ApprovalDecision(
                approved=True,
                approver="meetup-operator",
                reason="Approved interactively for the demo",
            )
        return ApprovalDecision(
            approved=False,
            approver="meetup-operator",
            reason="Approval rejected in the demo",
        )

    def _all_audit_entries(self) -> list[Any]:
        entries: list[Any] = []
        for governed_tool in self.governed_tools.values():
            if governed_tool.audit_log is not None:
                entries.extend(governed_tool.audit_log.query())
        return sorted(entries, key=lambda entry: entry.timestamp)

    def _latest_decision(self, tool_name: str) -> dict[str, Any]:
        entries = [entry for entry in self._all_audit_entries() if entry.action == tool_name]
        if not entries:
            return {"action": "allow", "reason": "Allowed by policy"}
        entry = entries[-1]
        return {
            "action": entry.policy_decision or entry.outcome,
            "reason": entry.data.get("reason", ""),
            "matched_rule": entry.data.get("rule") or entry.matched_rule,
        }


PROMPTS = {
    "1": ("Lookup customer", "Please look up customer C-1042"),
    "2": ("Send status email", "Please send the customer an email with a status update"),
    "3": ("Delete old record", "Please delete the old customer record"),
}


def print_result(prompt: str, result: dict[str, Any]) -> None:
    print(f"\nREQUEST: {prompt}")
    print(json.dumps(result, indent=2))


def print_audit(agent: GovernedAgent) -> None:
    print("\nAUDIT EVENTS FROM AGENT GOVERNANCE TOOLKIT:")
    entries = agent._all_audit_entries()
    if not entries:
        print("No audit events yet.")
        return
    for entry in entries:
        print(json.dumps(entry.model_dump(mode="json"), separators=(",", ":")))


def run_menu(agent: GovernedAgent) -> None:
    while True:
        print("\nChoose an agent request:")
        for key, (label, prompt) in PROMPTS.items():
            print(f"  {key}. {label}")
            print(f"     Request: {prompt}")
        print("  4. Show audit events")
        print("  q. Quit")
        try:
            choice = input("\nSelection: ").strip().lower()
        except (EOFError, KeyboardInterrupt):
            print("\nExiting.")
            return

        if choice in {"q", "quit", "exit"}:
            print("Exiting.")
            return
        if choice == "4":
            print_audit(agent)
            continue
        if choice not in PROMPTS:
            print("Please choose 1, 2, 3, 4, or q.")
            continue

        label, prompt = PROMPTS[choice]
        print(f"\nRunning: {label}")
        print_result(prompt, agent.run(prompt))


def main() -> int:
    parser = argparse.ArgumentParser(description="Run the Agent Governance Toolkit demo")
    parser.add_argument("--provider", choices=("auto", "mock", "foundry"), default="auto")
    parser.add_argument(
        "--once",
        choices=("lookup", "email", "delete"),
        help="Run one action without opening the interactive menu",
    )
    parser.add_argument(
        "--approve-email",
        action="store_true",
        help="Approve the email action when using --once email",
    )
    args = parser.parse_args()

    try:
        provider, provider_name = select_provider(args.provider)
        agent = GovernedAgent(provider)
        print(f"Provider: {provider_name}")
        print("Policy: policy.yaml (loaded by agentmesh.governance.govern)")
        if args.once:
            prompt = {
                "lookup": PROMPTS["1"][1],
                "email": PROMPTS["2"][1],
                "delete": PROMPTS["3"][1],
            }[args.once]
            print_result(prompt, agent.run(prompt, approve_escalation=args.approve_email))
        else:
            run_menu(agent)
        return 0
    except (RuntimeError, json.JSONDecodeError, KeyError) as error:
        print(f"Demo failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
