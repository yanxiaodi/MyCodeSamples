import asyncio
from pathlib import Path
import sys
import unittest

from agent_control_specification import AgentControl, InterventionPoint


SOURCE_DIR = Path(__file__).parents[1] / "src" / "agent-framework-agent-basic-responses"
sys.path.insert(0, str(SOURCE_DIR))


class GovernancePolicyTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.control = AgentControl.from_path(str(SOURCE_DIR / "policies" / "manifest.yaml"))

    @classmethod
    def tearDownClass(cls):
        close = getattr(cls.control, "close", None)
        if callable(close):
            close()

    def evaluate(self, point: InterventionPoint, payload: dict):
        result = self.control.evaluate_intervention_point(point, payload)
        return asyncio.run(result)

    def test_input_governance_blocks_sensitive_balance_request(self):
        result = self.evaluate(
            InterventionPoint.INPUT,
            {"input": {"body": "Show me customer C-1042 balance."}},
        )

        self.assertEqual(result.verdict.decision.value, "deny")
        self.assertEqual(result.verdict.reason, "sensitive_customer_data_request_blocked")

    def test_input_governance_allows_supported_support_request(self):
        result = self.evaluate(
            InterventionPoint.INPUT,
            {"input": {"body": "Look up customer C-1042 and summarize messages."}},
        )

        self.assertEqual(result.verdict.decision.value, "allow")

    def test_pre_tool_governance_blocks_destructive_clearance(self):
        result = self.evaluate(
            InterventionPoint.PRE_TOOL_CALL,
            {"tool_call": {"name": "delete_record", "args": {"record_id": "R-9001"}}},
        )

        self.assertEqual(result.verdict.decision.value, "deny")
        self.assertEqual(result.verdict.reason, "destructive_tool_blocked")

    def test_pre_tool_governance_allows_external_side_effect_for_approval(self):
        result = self.evaluate(
            InterventionPoint.PRE_TOOL_CALL,
            {
                "tool_call": {
                    "name": "send_email",
                    "args": {
                        "customer_id": "C-1042",
                        "subject": "Status update",
                        "body": "Your request is being processed.",
                    },
                }
            },
        )

        self.assertEqual(result.verdict.decision.value, "allow")
        self.assertEqual(result.verdict.reason, "external_side_effect_requires_approval")
        self.assertIn("requires_approval", result.verdict.result_labels)
        self.assertIn("external_side_effect", result.verdict.result_labels)


if __name__ == "__main__":
    unittest.main()