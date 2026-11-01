# AI Agent Governance and Observability in Practice

A 25-minute English talk by Xiaodi Yan. Governance controls an agent's actions;
observability explains its execution. One Python support agent demonstrates both.

- [Slides and English notes](slides/slides.md): 12 main slides plus 3 backups.
- [PDF for offline playback](slides/talk.pdf)
- [Presentation](http://localhost:4460/1) · [Presenter view](http://localhost:4460/presenter/1)
- [Browser demo](http://localhost:8090) · [Aspire](http://localhost:18988)
- [Rehearsal instructions](docs/rehearsal.md) · [Verified sources](docs/research/2026-11-01-genai-observability.md)

All deliverables live here. The earlier `AIGovernance` directory was not edited.

## Start the demo

Requirements: Python 3.12, Node.js 22 or newer, and Docker with its engine running.
Tested with Python 3.12, Node 24 and a pinned Aspire image. Install dependencies
before the talk; offline model mode then needs no cloud connection.

From this directory, in PowerShell:

```powershell
cd demo
py -3.12 -m venv .venv
.venv\Scripts\python.exe -m pip install -r requirements.lock.txt
docker compose up -d
.venv\Scripts\python.exe app.py
```

Open http://localhost:8090. In another terminal, from this directory:

```powershell
cd slides
npm ci
npm run dev
```

Open http://localhost:4460/1. Press `f` for fullscreen; use arrow keys to advance.
Presenter notes contain the English script, timing, animation builds and demo cues.
The deck uses staged policy flows, an OpenLLMetry/OTel architecture diagram,
a recorded trace waterfall and an animated Chart.js outcome comparison.
Charts read `docs/evidence/session.json`: nine offline rehearsal runs, with
synthetic timing and token usage. They are a fixed teaching example; the browser
demo and Aspire show current runs. Reduced-motion preferences disable movement.

| Service | Local address |
| --- | --- |
| Demo controls and OTel overview | http://localhost:8090 |
| Aspire diagnostic dashboard | http://localhost:18988 |
| OTLP HTTP receiver used by Python | http://localhost:43118 |
| OTLP gRPC, available but unused | http://localhost:43117 |
| Slidev | http://localhost:4460 |

Ports bind to loopback. Aspire runs anonymously for this local demonstration.
It is a development diagnostic viewer, not a production retention/audit platform.

## Two model modes

**Offline demo** uses the real OpenAI SDK and OpenLLMetry with an in-process mock
HTTP transport. Responses, latency and token counts are synthetic; AGT policy
evaluation, host enforcement and OTel pipelines actually execute.

**Foundry model** makes real Chat Completions calls with native function calling.
It needs a deployment supporting Chat Completions and tools. Both modes constrain
the first requested tool for repeatable scenarios. The host validates returned
arguments and evaluates policy before calling any tool body. Provider errors
never silently select mock mode.

Copy `demo/.env.example` to `demo/.env` and configure:

```dotenv
AZURE_AI_FOUNDRY_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/openai/v1/
AZURE_AI_FOUNDRY_API_KEY=YOUR-KEY
AZURE_AI_FOUNDRY_DEPLOYMENT=YOUR-CHAT-DEPLOYMENT
```

Restart the demo and select **Foundry model**. The CLI can also read an existing
configuration without modifying or copying it:

```powershell
cd demo
.venv\Scripts\python.exe run_demo.py --provider foundry --scenario all --env-file PATH-TO-EXISTING-ENV
```

All three Foundry scenarios were verified on 1 November 2026. Credentials are
excluded from this project. The current browser process reads the presenter's
existing configuration via `DEMO_ENV_FILE`; future starts use `demo/.env` by default.

## What executes

| Scenario | Policy | Runtime | Tool-body evidence |
| --- | --- | --- | --- |
| Allowed lookup | allow | succeeded | lookup = 1, deletion = 0 |
| Denied deletion | deny | blocked | lookup = 0, deletion = 0 |
| Lookup with injected timeout | allow | failed | lookup = 1, deletion = 0 |

Deletion has a tripwire body and never deletes anything. Records are fictional.
The failed scenario injects an 800 ms local tool timeout, not a Foundry outage.
Policy denial is a business outcome and does not mark its span as ERROR.

`demo/support_agent/agent.py` runs the bounded tool-calling loop. `policy.yaml`
is loaded by the official AGT `PolicyEngine`; the host enforces its decision.
OpenLLMetry's `opentelemetry-instrumentation-openai` emits model spans and metrics.
The application adds policy/tool spans, correlated logs and `app.*` business metrics.
No Traceloop account or Agent Framework runtime is required.

The application owns the OTel providers and exports over OTLP HTTP. Local evidence
comes from the same SDK pipelines rather than the displayed answer. The overview
composes counts and histogram means for the selected provider. Its 9-run warmup is
labelled demo traffic, not production percentile, cost or quality evidence.

Content and tool-argument capture is explicitly disabled. The browser keeps 50
results and 500 agent/model spans. HTTP polling spans remain in Aspire but do not
displace local evidence. Old runs expire in long sessions; restart Python for
clean counters. CLI runs append to `demo/artifacts/traces.jsonl`.

GenAI conventions are in **Development**. This instrumentation emits
`gen_ai.client.token.usage` and `gen_ai.client.operation.duration`; the newer
GenAI repository includes `gen_ai.client.inference.usage.*` token metrics.
These are versioned schemas; verify emitted names before upgrading.

## Verify and export

```powershell
cd demo
.venv\Scripts\python.exe run_demo.py --scenario all --no-otlp
.venv\Scripts\python.exe -m pytest -q
```

`--no-otlp` collects local CLI evidence without Docker. For browser mode without
Aspire set `DEMO_OTLP_ENABLED=false` before starting `app.py`.

```powershell
cd slides
npm run build
npm audit
```

Validated: 15 Python tests, three real Foundry scenarios, offline browser flows,
OTLP traces/logs/metrics in Aspire, and all 15 slide layouts at 1440×810.
Dependency deprecation warnings are recorded in the [ledger](docs/plans/progress.md).

The exported 15-page PDF is in `slides/talk.pdf`. Regenerate it at
http://localhost:4460/export. CLI export additionally requires
`playwright-chromium` as a dev dependency.
Stop Python/Slidev with Ctrl+C; stop Aspire with `docker compose stop` from `demo`.
