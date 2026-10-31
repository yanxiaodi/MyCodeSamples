# GenAI observability source checks

Checked on 1 November 2026 (Pacific/Auckland). These are source findings,
not claims that the new demo has been implemented or tested.

## OpenLLMetry

The linked repository contains Python provider/vector-store instrumentations
and a convenience SDK. Its README links a separate JS/TS repository, so
calling OpenLLMetry Python-only is inaccurate. No .NET SDK is advertised in
that README; this is not a claim about every Traceloop product.

Traceloop SaaS is optional: the SDK accepts custom exporters or a different
endpoint. Bare `Traceloop.init()` targets Traceloop and requires a key, so
configure a local demo destination explicitly.

Sources:

- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/README.md
- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/packages/traceloop-sdk/traceloop/sdk/__init__.py
- https://github.com/traceloop/openllmetry-js

## Responses API and metrics

The inspected instrumentor wraps Responses create/retrieve/parse/cancel and
async equivalents. Responses spans include model and token-usage attributes.
This matches the API used by the earlier local demo.

Responses wrappers receive a tracer, whereas chat-completions wrappers also
receive metric instruments. Do not assume automatic Responses token and
duration metrics: verify the pinned release and add metrics where needed.
The SDK disables its metrics pipeline if a custom trace exporter/processor
is supplied without a custom metric exporter. Trace export alone is insufficient.

Sources:

- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/packages/opentelemetry-instrumentation-openai/opentelemetry/instrumentation/openai/v1/__init__.py
- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/packages/opentelemetry-instrumentation-openai/opentelemetry/instrumentation/openai/v1/responses_wrappers.py
- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/packages/traceloop-sdk/traceloop/sdk/__init__.py

## GenAI semantic conventions

The official repository defines spans, metrics, and events for clients,
agents, MCP, and specific providers, and includes reference implementations.
Explain its role as a shared vocabulary, distinct from instrumentation and visualization.

The overview and inspected signal documents explicitly have Development
status. Instrumentations may emit different convention versions. Inspected
OpenLLMetry declares `gen_ai.client.token.usage` and
`gen_ai.client.operation.duration`; the new GenAI repository defines inference
token counters under `gen_ai.client.inference.usage.*` and separate inference
duration metrics. Pin versions and document actual output rather than
claiming full compliance with the latest draft.

Sources:

- https://github.com/open-telemetry/semantic-conventions-genai/blob/6f98aef121ae3d5f224c5084d9fb4433bc6f7f67/README.md
- https://github.com/open-telemetry/semantic-conventions-genai/blob/6f98aef121ae3d5f224c5084d9fb4433bc6f7f67/docs/gen-ai/README.md
- https://github.com/open-telemetry/semantic-conventions-genai/blob/6f98aef121ae3d5f224c5084d9fb4433bc6f7f67/docs/gen-ai/gen-ai-metrics.md
- https://github.com/open-telemetry/semantic-conventions-genai/blob/6f98aef121ae3d5f224c5084d9fb4433bc6f7f67/docs/gen-ai/gen-ai-token-metrics.md
- https://github.com/traceloop/openllmetry/blob/8d9c93f37b352131f5871dcb6ecf5e79c19aeb40/packages/opentelemetry-semantic-conventions-ai/opentelemetry/semconv_ai/__init__.py

## Dashboard

Aspire standalone accepts OTLP traces, structured logs, and metrics from
instrumented applications including Python, without an AppHost. Starting it
does not instrument code. It is intended for development and short-term
diagnostics rather than long-term production telemetry retention.

Source: https://aspire.dev/dashboard/standalone/

## Local references

The earlier `AIGovernance/demo/governed_agent.py` uses Python AGT, OpenAI
Responses, a deterministic mock, governed tools, and callable audit logs.
The hosted Python sample already configures Agent Framework OTel or Azure
Monitor and enables sensitive capture. Revisit capture settings for the new
demo; use synthetic data and deliberate content opt-in.

Original discussion:
https://chatgpt.com/share/6ae6659d-4188-83ec-a96b-19d30c8e69e7
