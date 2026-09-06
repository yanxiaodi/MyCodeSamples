from pathlib import Path
import sys
import unittest


SOURCE_DIR = Path(__file__).parents[1] / "src" / "agent-framework-agent-basic-responses"
sys.path.insert(0, str(SOURCE_DIR))

from tools import DemoEmailSender, delete_record, lookup_customer_messages, send_email  # noqa: E402


class RecordingEmailSender(DemoEmailSender):
    def __init__(self):
        self.messages = []

    def send(self, *, to: str, subject: str, body: str) -> dict:
        message = {"to": to, "subject": subject, "body": body}
        self.messages.append(message)
        return {"status": "sent", "mock": True, **message}


class DemoToolsTests(unittest.TestCase):
    def test_lookup_customer_messages_returns_demo_data(self):
        result = lookup_customer_messages("C-1042")

        self.assertEqual(result["customer_id"], "C-1042")
        self.assertTrue(result["messages"])

    def test_send_email_uses_the_injected_sender(self):
        sender = RecordingEmailSender()

        result = send_email(
            to="customer@example.com",
            subject="Status update",
            body="Your request is being processed.",
            sender=sender,
        )

        self.assertEqual(result["status"], "sent")
        self.assertEqual(sender.messages[0]["to"], "customer@example.com")

    def test_delete_record_is_a_safe_demo_operation(self):
        result = delete_record("R-9001")

        self.assertEqual(
            result,
            {
                "record_id": "R-9001",
                "deleted": True,
                "mock": True,
            },
        )
