# Verification: 1 November 2026

- Python: 15 tests passed; `pip check` found no broken requirements.
- Slidev production build successful; `npm audit` found 0 vulnerabilities.
- Browser: all 15 pages checked at 1440×810, no content outside slide bounds.
- PDF: 15 pages, 720×404.88 pt; text extracted on every page. All pages rendered
  with Poppler and inspected in `pdf-contact-sheet.png`; no layout defects.
- Independent read-only review: no material findings in enforcement, claims,
  provider dimensions or recorded totals.
- Git reports no changes in the original `AIGovernance` directory.

## Recorded behavior

`session.json`: nine offline runs (three of each scenario), 36 agent/model spans,
27 correlated business logs. Policy: 6 allow, 3 deny. Runtime: 3 succeeded,
3 blocked, 3 failed. Model metrics: 1,140 fixture input tokens + 258 output = 1,398.
Every deletion-body execution counter is zero.

Real Foundry CLI calls, with existing configuration read without copying credentials:

| Scenario | Decision | Outcome | Lookup/deletion counts | Trace ID |
| --- | --- | --- | --- | --- |
| allowed | allow | succeeded | 1 / 0 | 826c60a8eefc9502175d065bf5c7984f |
| blocked | deny | blocked | 0 / 0 | ae89783840c835314b7cbc7877bf7f6a |
| failed | allow | failed, TimeoutError | 1 / 0 | 79672b053cc448ab5ccb02179cb16379 |

Allowed used 472 input and 62 output tokens across two calls. Blocked/failed each
used one call. Failure is an injected 800 ms local timeout, not a Foundry outage.
These are functional checks, not latency benchmarks.

## Aspire

`aspire-trace-allowed.png` records the actual HTTP parent, agent, two model calls,
policy and tool: trace `ac97bd3e45927eb32e3b538daf52a424`.
`aspire-metrics.png` records the actual policy instrument and provider, decision,
rule and tool dimensions. Aspire's multiple-instance view differs from the
browser's current-process overview. Actual OTLP traces, structured logs and all
model/business metric instruments were received. Local evidence filters HTTP
polling spans; Aspire also shows HTTP/dependency/serialization spans from FastAPI.

## Limits

Synthetic records; constrained first tool choice; bounded history; diagnostic
retention. Prompt/tool payload capture is off. Traces are not durable auditing
or independent proof of response quality.

Six deprecation warnings come from Starlette's httpx TestClient, three AGT package
consolidation imports, OTel SDK LoggingHandler and OpenLLMetry's Pydantic config.
They do not fail tests. FloatingVue was pinned to 5.2.2 for Slidev compatibility;
the final slide browser console has no errors.
