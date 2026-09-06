"""Tools used by the governance demo.

The lookup and delete tools deliberately use deterministic demo data. The email
tool is the only tool with an external side effect and sends through Azure
Communication Services when it is actually approved and invoked.
"""

from __future__ import annotations

import os
import logging
from typing import Any


logger = logging.getLogger("agent_governance_demo.tools")


class DemoEmailSender:
    """Small seam that keeps the email tool easy to test without Azure."""

    def send(self, *, to: str, subject: str, body: str) -> dict[str, Any]:
        raise NotImplementedError


class AzureCommunicationEmailSender(DemoEmailSender):
    """Send an email through an Azure Communication Services resource."""

    def __init__(self, connection_string: str, sender_address: str) -> None:
        self._connection_string = connection_string
        self._sender_address = sender_address

    def send(self, *, to: str, subject: str, body: str) -> dict[str, Any]:
        from azure.communication.email import EmailClient

        client = EmailClient.from_connection_string(self._connection_string)
        message = {
            "content": {
                "subject": subject,
                "plainText": body,
            },
            "recipients": {
                "to": [{"address": to}],
            },
            "senderAddress": self._sender_address,
        }
        poller = client.begin_send(message)
        result = poller.result()
        message_id = getattr(result, "id", None)
        return {
            "status": "sent",
            "provider": "azure-communication-services",
            "message_id": message_id,
            "to": to,
        }


def lookup_customer_messages(customer_id: str) -> dict[str, Any]:
    """Return deterministic mock customer messages for the demo."""

    result = {
        "customer_id": customer_id,
        "messages": [
            {
                "id": "M-3001",
                "subject": "Support request",
                "body": "Customer asked for an update on their application.",
                "status": "open",
            },
            {
                "id": "M-3002",
                "subject": "Verification complete",
                "body": "Identity verification completed successfully.",
                "status": "closed",
            },
        ],
        "mock": True,
    }
    logger.info(
        "tool_call name=lookup_customer_messages result=mock message_count=%d",
        len(result["messages"]),
    )
    return result


def send_email(
    to: str,
    subject: str,
    body: str,
    *,
    sender: DemoEmailSender | None = None,
) -> dict[str, Any]:
    """Send a customer status email through Azure Communication Services."""

    if sender is None:
        connection_string = os.environ.get("AZURE_COMMUNICATION_SERVICES_CONNECTION_STRING")
        sender_address = os.environ.get("AZURE_COMMUNICATION_SERVICES_SENDER")
        if not connection_string or not sender_address:
            raise RuntimeError(
                "Email is enabled, but Azure Communication Services is not configured. "
                "Set AZURE_COMMUNICATION_SERVICES_CONNECTION_STRING and "
                "AZURE_COMMUNICATION_SERVICES_SENDER."
            )
        sender = AzureCommunicationEmailSender(connection_string, sender_address)

    logger.info("tool_call name=send_email status=started")
    try:
        result = sender.send(to=to, subject=subject, body=body)
    except Exception:
        logger.exception("tool_call name=send_email status=failed")
        raise
    logger.info(
        "tool_call name=send_email status=%s",
        result.get("status", "unknown"),
    )
    return result


def delete_record(record_id: str) -> dict[str, Any]:
    """Return a mock deletion result; no real record is changed."""

    result = {
        "record_id": record_id,
        "deleted": True,
        "mock": True,
    }
    logger.info("tool_call name=delete_record result=mock deleted=%s", result["deleted"])
    return result
