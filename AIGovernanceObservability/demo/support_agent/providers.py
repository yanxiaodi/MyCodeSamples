"""Both modes use the real SDK. Only mock mode replaces its HTTP transport."""
from __future__ import annotations

import json
import os
import time
from dataclasses import dataclass, field
from typing import Any, Protocol

import httpx
from openai import OpenAI


class InvalidToolCall(ValueError):
    pass


class ModelProviderError(RuntimeError):
    pass


@dataclass
class ToolCall:
    name: str
    arguments: dict[str, Any]
    id: str


@dataclass
class ModelTurn:
    text: str
    calls: list[ToolCall] = field(default_factory=list)
    input_tokens: int = 0
    output_tokens: int = 0


class Provider(Protocol):
    mode: str
    def complete(self, messages: list[dict], tool_choice: Any = None) -> ModelTurn: ...


TOOL_DEFINITIONS = [
    {"type": "function", "function": {
        "name": name, "description": description,
        "parameters": {"type": "object", "properties": {
            "customer_id": {"type": "string", "description": "The synthetic customer identifier."}
        }, "required": ["customer_id"], "additionalProperties": False},
        "strict": True,
    }}
    for name, description in [
        ("lookup_customer", "Read a synthetic customer's support status."),
        ("delete_record", "Request deletion of a customer record. The host must enforce policy."),
    ]
]


class SDKProvider:
    mode = "foundry"

    def __init__(self, client: OpenAI, model: str):
        self.client, self.model = client, model

    def complete(self, messages, tool_choice=None):
        try:
            response = self.client.chat.completions.create(
                model=self.model, messages=messages, tools=TOOL_DEFINITIONS,
                tool_choice=tool_choice or "none", parallel_tool_calls=False,
            )
        except Exception as exc:
            # Do not copy provider response bodies, credentials or prompts into app logs.
            raise ModelProviderError(
                f"Model request failed ({type(exc).__name__}). Check the model configuration and connectivity."
            ) from None
        if not response.choices:
            raise ModelProviderError("The model returned no choices.")
        message = response.choices[0].message
        calls = []
        for call in message.tool_calls or []:
            try:
                arguments = json.loads(call.function.arguments)
            except (ValueError, TypeError):
                raise InvalidToolCall("The model returned invalid tool arguments.") from None
            calls.append(ToolCall(call.function.name, arguments, call.id))
        usage = response.usage
        return ModelTurn(message.content or "", calls,
                         usage.prompt_tokens if usage else 0,
                         usage.completion_tokens if usage else 0)

    def close(self):
        self.client.close()


class MockProvider(SDKProvider):
    mode = "mock"

    def __init__(self):
        client = OpenAI(
            api_key="offline-demo-not-a-secret", base_url="http://mock-provider.local/v1",
            max_retries=0, http_client=httpx.Client(transport=httpx.MockTransport(self._respond)),
        )
        super().__init__(client, "demo-model")

    @staticmethod
    def _respond(request: httpx.Request) -> httpx.Response:
        data = json.loads(request.content)
        time.sleep(0.12)
        choice = data.get("tool_choice")
        if isinstance(choice, dict):
            message = {"role": "assistant", "content": None, "tool_calls": [{
                "id": "call_demo", "type": "function", "function": {
                    "name": choice["function"]["name"],
                    "arguments": json.dumps({"customer_id": "C-1042"}),
                }}]}
            finish, prompt_tokens, completion_tokens = "tool_calls", 80, 18
        else:
            tool_message = next((m for m in reversed(data["messages"]) if m["role"] == "tool"), None)
            customer = json.loads(tool_message["content"]) if tool_message else {}
            message = {"role": "assistant", "content": (
                f"{customer.get('name', 'The customer')} has an active account. "
                "Their support request is awaiting a document review."
            )}
            finish, prompt_tokens, completion_tokens = "stop", 140, 32
        return httpx.Response(200, json={
            "id": "chatcmpl-offline-demo", "object": "chat.completion", "created": 1793480400,
            "model": "demo-model", "choices": [{"index": 0, "message": message, "finish_reason": finish}],
            "usage": {"prompt_tokens": prompt_tokens, "completion_tokens": completion_tokens,
                      "total_tokens": prompt_tokens + completion_tokens},
        })


def create_provider(mode: str) -> SDKProvider:
    if mode == "mock":
        return MockProvider()
    if mode != "foundry":
        raise ValueError("Choose mock or foundry explicitly.")
    names = ["AZURE_AI_FOUNDRY_ENDPOINT", "AZURE_AI_FOUNDRY_API_KEY", "AZURE_AI_FOUNDRY_DEPLOYMENT"]
    missing = [name for name in names if not os.getenv(name)]
    if missing:
        raise ModelProviderError("Missing Foundry configuration: " + ", ".join(missing))
    endpoint = os.environ[names[0]].rstrip("/")
    if endpoint.endswith("/responses"):
        endpoint = endpoint.removesuffix("/responses")
    client = OpenAI(base_url=endpoint, api_key=os.environ[names[1]], timeout=20.0, max_retries=0)
    return SDKProvider(client, os.environ[names[2]])
