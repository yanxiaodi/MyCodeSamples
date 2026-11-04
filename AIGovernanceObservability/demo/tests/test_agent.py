"""Tests name behavior at the model-to-tool boundary, using the real AGT policy."""
import pytest


def test_allowed_lookup_executes_and_returns_synthetic_customer():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    result = AgentRunner(MockProvider()).run("allowed", principal=DEMO_PRINCIPALS["standard"])
    assert result.outcome == "succeeded"
    assert result.policy_decision == "allow"
    assert result.principal_role == "standard_user"
    assert result.tool_body_counts == {"lookup_customer": 1, "delete_record": 0}
    assert "Avery Example" in result.answer


def test_standard_user_deletion_is_denied_before_tool_body():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    result = AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["standard"])
    assert result.outcome == "blocked"
    assert result.policy_decision == "deny"
    assert result.principal_role == "standard_user"
    assert result.rule == "deny-non-admin-deletion"
    assert result.tool_body_counts == {"lookup_customer": 0, "delete_record": 0}


def test_admin_deletion_is_allowed_but_only_simulated():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    result = AgentRunner(MockProvider()).run("delete", principal=DEMO_PRINCIPALS["admin"])
    assert result.outcome == "succeeded"
    assert result.policy_decision == "allow"
    assert result.principal_role == "support_admin"
    assert result.rule == "allow-admin-deletion"
    assert result.tool_body_counts == {"lookup_customer": 0, "delete_record": 1}
    assert "simulated" in result.answer.lower()
    assert "no customer record was changed" in result.answer.lower()


def test_allowed_lookup_timeout_remains_a_runtime_failure():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS
    from support_agent.providers import MockProvider

    result = AgentRunner(MockProvider(), timeout_seconds=0.01).run(
        "failed", principal=DEMO_PRINCIPALS["standard"],
    )
    assert result.outcome == "failed"
    assert result.policy_decision == "allow"
    assert result.error_type == "TimeoutError"
    assert result.tool_body_counts == {"lookup_customer": 1, "delete_record": 0}


class FixedToolProvider:
    mode = "test"

    def __init__(self, name, arguments):
        self.name, self.arguments = name, arguments

    def complete(self, messages, tool_choice=None):
        from support_agent.providers import ModelTurn, ToolCall
        return ModelTurn(text="", calls=[ToolCall(self.name, self.arguments, "call_test")], input_tokens=4, output_tokens=3)


def test_unregistered_demo_identity_cannot_claim_admin_role():
    from support_agent.agent import AgentRunner, DemoPrincipal

    forged_principal = DemoPrincipal(
        "standard", "support_admin", "Standard user",
        frozenset({"customer:read", "customer:delete"}),
    )
    with pytest.raises(ValueError, match="supported simulated identity"):
        AgentRunner(FixedToolProvider("delete_record", {"customer_id": "C-1042"})).run(
            "delete", principal=forged_principal,
        )


def test_unknown_model_tool_is_rejected_before_execution():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS

    result = AgentRunner(FixedToolProvider("shell_exec", {"command": "anything"})).run(
        "allowed", principal=DEMO_PRINCIPALS["standard"],
    )
    assert result.outcome == "failed"
    assert result.error_type == "InvalidToolCall"
    assert sum(result.tool_body_counts.values()) == 0


def test_model_arguments_cannot_override_the_governance_action():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS

    provider = FixedToolProvider("delete_record", {"customer_id": "C-1042", "action": "lookup_customer"})
    result = AgentRunner(provider).run("delete", principal=DEMO_PRINCIPALS["standard"])
    assert result.outcome == "failed"
    assert result.error_type == "InvalidToolCall"
    assert sum(result.tool_body_counts.values()) == 0


def test_missing_customer_id_is_rejected_before_tool_execution():
    from support_agent.agent import AgentRunner, DEMO_PRINCIPALS

    result = AgentRunner(FixedToolProvider("lookup_customer", {})).run(
        "allowed", principal=DEMO_PRINCIPALS["standard"],
    )
    assert result.outcome == "failed"
    assert result.error_type == "InvalidToolCall"
    assert sum(result.tool_body_counts.values()) == 0
