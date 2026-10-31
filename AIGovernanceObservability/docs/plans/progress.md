# Implementation ledger: 2026-11-01-talk-and-demo.md

The user authorized implementation on 1 November 2026. Dashboard defaults to Aspire standalone; the demo uses Python and an SDK-level agent so OpenLLMetry owns model instrumentation.

The user explicitly requested a new project directory. All deliverables stay in this directory; no worktree migration, commits, deployment, email or deletion is needed.

Pre-flight: provider adapters produce ToolCall values; the governance loop consumes them. The API and CLI share run_scenario. Telemetry is initialized before provider construction. Slides consume sanitized rehearsal evidence.

- Setup: Docker engine accessible after sandbox escalation; Python 3.12 selected for dependency compatibility.
- Task 1: complete. Six governance behavior tests passed using the official AGT policy engine.
- Task 2: complete. Trace parentage, denial without execution, metrics, content exclusion, concurrent isolation and polling-noise retention verified. Three browser API tests failed before implementation and passed afterward.
- Implementation decision: use AGT's public PolicyEngine.evaluate and explicit host enforcement rather than its combined govern wrapper, so policy and execution are distinct spans. Tests enforce the deny-before-body contract. The library is pinned for reproducibility.
- Implementation decision: use Chat Completions native function calls for this demo, whose OpenLLMetry instrumentation emits both spans and metrics. Responses support remains in the research notes; it is not the demonstrated API.
- Verification: initial suite 11 passed, with five upstream deprecation warnings. No application test failures.
- Verification: full suite 14 passed, with six dependency deprecation warnings.
- Environment: Windows excludes 14254-14353 and 3418-3517 from socket binding. OTLP moved to 43117/43118 and slides to 4460; Aspire UI remains 18988.
- Task 3: complete. 12 main slides and 3 backups, timed English notes, offline fallback, actual browser/Aspire screenshots, runbook and 15-page PDF.
- Task 4: complete. Final 15 tests passed; pip check passed; Slidev build passed; npm audit found 0 vulnerabilities. PDF rendered and visually checked. Independent reviewer found no material issues. All three real Foundry scenarios passed.
- Six dependency deprecation warnings remain documented in docs/evidence/verification.md. FloatingVue pinned to 5.2.2; final browser console has no errors.
- Recorded evidence: 9 offline runs, 6 allows, 3 denials, 3 failures, 1398 synthetic tokens, zero deletion-body executions. Actual OTLP traces/logs/metrics received by Aspire.
- README includes commands and links. Original AIGovernance is unchanged. Initial delivery included no commit or deployment.
- User authorized a PR. Slides were rebuilt successfully before submission, and tests were rerun. Per-page rendering check PNGs are excluded; source, representative evidence and the optional offline PDF are included.
