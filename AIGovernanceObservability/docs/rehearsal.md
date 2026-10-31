# Rehearsal and live-demo cues

Target 25 minutes including 9 minutes of demo and 2 minutes for closing/questions.
Keep backups outside the timed path. Presenter notes are English.

| Clock | Slides | Delivery |
| --- | --- | --- |
| 00:00–02:00 | 1–2 | The final response does not explain the execution. |
| 02:00–05:00 | 3–4 | Parallel capabilities; host enforcement at the action boundary. |
| 05:00–08:00 | 5 | Parent/child spans, duration, outcomes and three signals. |
| 08:00–11:00 | 6–7 | Conventions, instrumentation, export and dashboard. |
| 11:00–14:00 | 8 + UI | Allowed: two model calls, policy allow, one tool body. |
| 14:00–17:00 | 9 + UI | Blocked: policy deny, zero body executions. |
| 17:00–20:00 | 10 + UI | Failed: policy allow, tool timeout and correlated evidence. |
| 20:00–23:00 | 11 | Context, content, outcomes and completeness in production. |
| 23:00–25:00 | 12 | Takeaways and questions. |

## Before the audience arrives

1. Start Aspire, Python and Slidev using the README commands.
2. Keep slides, localhost:8090 and localhost:18988 in separate tabs.
3. Use offline mode for the timed demo. Click **Generate 9 offline demo runs** once.
   Expected deltas: 6 allows, 3 denials, 3 failures, 1,398 synthetic tokens.
   Warmup always switches to offline mode.
4. Check all three outcomes and execution counters. In Aspire choose resource
   governed-support-agent; clear earlier filters.
5. For optional real inference, select Foundry and rehearse a lookup first.
   Say the model is real, records are fictional, and first-tool selection is constrained.
6. Keep screenshots and backup slide 14 ready. Switch to offline mode or recorded
   evidence if a cloud dependency delays the talk.

Restart Python for clean counters. Aspire keeps its own in-memory session until
the container restarts; use a trace ID to locate a specific current run.

## Allowed — slide 8, three minutes

Say: “The model requests a customer lookup. Our host evaluates that request before
the tool runs.” Click **Run allowed lookup**. Explain the answer, matched allow
rule, lookup-body counter of 1, and deletion counter of 0.

Read the local tree: invoke_agent support-agent, first model call,
policy.evaluate lookup_customer, execute_tool lookup_customer, second model call.
All share one trace ID.

Click **Inspect traces in Aspire** to open this exact trace. You can also copy its
ID and add a `TraceId` filter. The enclosing HTTP request may appear above the agent span.
Select a model call and show its model/token attributes. Content is not captured.

## Blocked — slide 9, three minutes

Say: “A denial is different from a broken system.” Click **Request denied deletion**.
Point to deny, rule block-destructive, and deletion-body count 0. The policy span
is not ERROR. A tool execution span is absent.

A missing span alone cannot prove non-execution. Here the same host boundary also
checks actual tool-body entry counters. Show the correlated deny log. An ordinary
retry should not bypass the boundary.

## Failed — slide 10, three minutes

Say: “Permission does not guarantee success.” Click **Run lookup with timeout**.
Policy is allow, runtime failed, lookup-body count 1, deletion 0. Explain the
injected local timeout; it is not a measured provider outage.

In Aspire select its tool span: ERROR, TimeoutError, approximately 800 ms.
The policy span shows allow with no error. Show Structured logs for this trace,
then Metrics: app.governance.decisions, app.agent.runs, app.tool.duration,
and gen_ai.client.token.usage.

The browser composes these SDK metrics into counts and means. Allowed tool
requests and successful runs intentionally have different totals.

## Fallback and questions

- No internet: offline mode executes all flows without cloud calls.
- No Docker: browser DEMO_OTLP_ENABLED=false or CLI --no-otlp; use local evidence
  and recorded Aspire screenshots.
- No live UI: backup slide 14 and docs/evidence contain recorded evidence.
- Approvals: backup slide 15 discusses binding approval to agent, tool, arguments
  and policy version. This demo does not implement or send approval requests.
- Standards: refer to sources; this pinned OpenLLMetry release does not imply
  support for every current GenAI draft field.

Keep identity, durable auditing and evaluation to the production slide unless asked.
