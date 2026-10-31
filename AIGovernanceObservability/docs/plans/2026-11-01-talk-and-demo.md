# Governance and Observability Talk Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Deliver a runnable Python demonstration and a 25-minute English Slidev deck about governing and observing one support agent.

**Architecture:** A small tool-calling agent uses the OpenAI-compatible SDK and official AGT tool wrappers. OpenLLMetry instruments the SDK; explicit application spans, structured logs, and metrics cover policy and tool execution. A local Aspire standalone dashboard receives OTLP; the demo also writes sanitized rehearsal evidence for offline inspection.

**Tech Stack:** Python 3.12, OpenAI SDK, agent-governance-toolkit-core, OpenLLMetry OpenAI instrumentation, OpenTelemetry Python, FastAPI, Aspire standalone, Slidev/Vue.

**Spec:** ../../README.md and the approved design discussion in this chat.

## Global constraints

- Work only in `AIGovernanceObservability`; do not modify the earlier talk.
- English slides and demo interface; approximately 25 minutes, including 9 minutes of demo.
- Governance and observability are parallel; identity, auditing, evaluation receive brief context only.
- Synthetic customer data; no real deletion or email side effects.
- Explicit provider mode: mock runs are labelled, and Foundry failures do not silently become successful mock runs.
- Prompts and completion content are not captured by default.
- Export traces, logs, and metrics; policy denial is a separate outcome from runtime error.
- Configure one owner for model-call instrumentation; custom business fields use `app.*`.
- Aspire is the default viewer; no Traceloop account or SaaS endpoint is required.
- Leave git integration to the user; retain verification evidence in this project.

## Review focus

- Unknown tools or malformed model arguments must fail before any tool body executes.
- A denied deletion must not run even if the model explicitly requests it.
- A tool timeout must retain the allow decision and appear as a runtime error.
- Concurrent requests must not mix trace IDs, decisions, or execution counts.
- Foundry configuration/export failures must be visible and must not leak credentials or falsely report success.

## File structure

- `demo/support_agent/agent.py`: tool-call loop, validated arguments, official AGT boundary and outcomes.
- `demo/support_agent/providers.py`: deterministic mock and configured Foundry SDK adapter.
- `demo/support_agent/telemetry.py`: OTel providers, OpenLLMetry SDK instrumentation, policy/application signals, sanitized evidence exporter.
- `demo/support_agent/tools.py`: synthetic customer lookup and controlled timeout; deletion execution counter.
- `demo/app.py`: browser demo controls and API; no business logic in routes.
- `demo/index.html`: three scenario buttons, outcomes and links to Aspire.
- `demo/run_demo.py`: CLI scenarios and warm-up traffic.
- `demo/compose.yaml`: standalone local Aspire dashboard.
- `demo/tests/`: real governance, behavior and telemetry contract tests.
- `slides/slides.md`, `slides/styles/index.css`: 12 main slides, backup references, English notes.
- `docs/evidence/`: verification report and representative rehearsal output.

## Task 1: Governance and runnable scenarios

Interfaces: `AgentRunner(provider).run(scenario) -> RunResult`, with outcome, policy decision, execution count, trace ID and answer. `ToolCall(name, arguments, id)` is the provider contract.

- [x] Write failing tests for allow, deny-before-body, allow-then-timeout, unknown tools and malformed arguments.
- [x] Run the tests and observe missing behavior.
- [x] Implement synthetic tools, provider adapters and the agent with official AGT enforcement.
- [x] Run the whole demo test suite; preserve denial and failure as distinct results.

## Task 2: Telemetry, dashboard and browser controls

Consumes Task 1 outcomes. Produces correlated OTel spans/logs/metrics and a JSON API for the browser demo.

- [x] Write failing tests for trace parentage, policy outcomes, token/runtime metrics and content exclusion.
- [x] Implement OTLP pipelines, selected OpenLLMetry OpenAI instrumentation, and local sanitized evidence.
- [x] Implement CLI, browser controls and Aspire Compose setup.
- [x] Run the test suite, start Aspire, export three scenarios and verify traces, logs and metrics in its UI.

## Task 3: Slides, speaker notes and rehearsal

Consumes real demo evidence from Task 2.

- [x] Create Slidev deck with 12 main slides, sources/backup content, and time-budgeted English speaker notes.
- [x] Build the deck and inspect all slides in the browser for clipping and legibility.
- [x] Capture representative demo screenshots and provide an offline fallback plus a runbook.
- [x] Verify documented startup commands and record actual limitations.

## Task 4: Final review and delivery

- [x] Run the complete demo tests and slide build.
- [x] Obtain a fresh review of this project's implementation and claims; address material findings.
- [x] Update README with commands and links, record verification results, and open the slides and demo for the user.
