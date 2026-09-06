from pathlib import Path
import sys
import unittest

from agent_framework import (
    AgentContext,
    Content,
    FunctionInvocationContext,
    MiddlewareTermination,
    ResponseStream,
)


SOURCE_DIR = Path(__file__).parents[1] / "src" / "agent-framework-agent-basic-responses"
sys.path.insert(0, str(SOURCE_DIR))

from main import _AgentControlInputMiddleware, _AgentControlToolMiddleware  # noqa: E402


class FakeVerdict:
    def __init__(self, *, reason: str, result_labels: tuple[str, ...]):
        self.reason = reason
        self.result_labels = result_labels


class FakeResult:
    transformed_policy_target_applied = False
    transformed_policy_target = None

    def __init__(self, *, result_labels: tuple[str, ...]):
        self.verdict = FakeVerdict(
            reason="external_side_effect_requires_approval",
            result_labels=result_labels,
        )


class FakeControl:
    def __init__(self, *, result_labels: tuple[str, ...]):
        self.result_labels = result_labels

    async def evaluate_intervention_point(self, *_args):
        return FakeResult(result_labels=self.result_labels)

    async def enforce(self, *_args):
        return None


class DenyingFakeControl(FakeControl):
    async def enforce(self, *_args):
        raise RuntimeError("blocked_by_test_policy")


class FakeFunction:
    name = "send_email"


class FakeAgent:
    pass


class GovernanceMiddlewareTests(unittest.IsolatedAsyncioTestCase):
    def make_context(self, *, approval_response: Content | None = None):
        metadata = {
            "call_id": "call-1",
            "function_call_occurrence_id": "occurrence-1",
        }
        if approval_response is not None:
            metadata["approval_response"] = approval_response
        return FunctionInvocationContext(
            function=FakeFunction(),
            arguments={"customer_id": "C-1042", "subject": "Status", "body": "Hello"},
            metadata=metadata,
        )

    async def test_policy_approval_label_creates_maf_approval_request(self):
        middleware = _AgentControlToolMiddleware(
            FakeControl(result_labels=("requires_approval", "external_side_effect"))
        )
        context = self.make_context()

        async def call_next():
            raise AssertionError("tool should not execute before approval")

        with self.assertRaises(MiddlewareTermination):
            await middleware.process(context, call_next)

        self.assertEqual(context.result.type, "function_approval_request")
        self.assertEqual(context.result.id, "occurrence-1")
        self.assertEqual(context.result.function_call.call_id, "call-1")
        self.assertEqual(context.result.function_call.id, "occurrence-1")
        self.assertEqual(context.result.additional_properties["governance_policy"], True)

    async def test_approved_policy_request_allows_tool_execution(self):
        function_call = Content.from_function_call(
            call_id="call-1",
            name="send_email",
            arguments={"customer_id": "C-1042", "subject": "Status", "body": "Hello"},
            id="occurrence-1",
        )
        approval_response = Content.from_function_approval_response(
            approved=True,
            id="occurrence-1",
            function_call=function_call,
        )
        middleware = _AgentControlToolMiddleware(
            FakeControl(result_labels=("requires_approval", "external_side_effect"))
        )
        context = self.make_context(approval_response=approval_response)
        executed = False

        async def call_next():
            nonlocal executed
            executed = True

        await middleware.process(context, call_next)

        self.assertTrue(executed)

    async def test_input_policy_denial_returns_stream_for_streaming_run(self):
        middleware = _AgentControlInputMiddleware(DenyingFakeControl(result_labels=()))
        context = AgentContext(
            agent=FakeAgent(),
            messages=[],
            stream=True,
        )

        async def call_next():
            raise AssertionError("agent should not run after input denial")

        with self.assertRaises(MiddlewareTermination):
            await middleware.process(context, call_next)

        self.assertIsInstance(context.result, ResponseStream)
        updates = [update async for update in context.result]
        self.assertEqual(updates[0].contents[0].text, "⛔ Policy violation: external_side_effect_requires_approval")

    async def test_input_policy_denial_returns_agent_response_for_non_streaming_run(self):
        middleware = _AgentControlInputMiddleware(DenyingFakeControl(result_labels=()))
        context = AgentContext(
            agent=FakeAgent(),
            messages=[],
            stream=False,
        )

        async def call_next():
            raise AssertionError("agent should not run after input denial")

        with self.assertRaises(MiddlewareTermination):
            await middleware.process(context, call_next)

        self.assertEqual(context.result.text, "⛔ Policy violation: external_side_effect_requires_approval")


if __name__ == "__main__":
    unittest.main()