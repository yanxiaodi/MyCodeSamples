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
| 14:00–17:00 | 9 + UI | Same delete request: standard user denied; admin runs a safe simulation. |
| 17:00–20:00 | 10 + UI | Failed: policy allow, tool timeout and correlated evidence. |
| 20:00–23:00 | 11 | Context, content, outcomes and completeness in production. |
| 23:00–25:00 | 12 | Takeaways and questions. |

## Click builds

Use Right Arrow to advance a build and Left Arrow to revisit it. The next slide
appears after the last build. Each build replaces part of the explanation; keep
the original time budget rather than adding time for the animations.

| Slide | Clicks | Visual progression |
| --- | --- | --- |
| 1 | 0 | Two parallel lanes draw once on entry. |
| 2 | 2 | Ask whether to retry, then reveal blocked and failed. |
| 3 | 0 | Two capabilities, equal emphasis. |
| 4 | 3 | Policy evaluation, decision, then host execution/stop. |
| 5 | 4 | Model, policy, tool, then the tool's share of this trace. |
| 6 | 2 | Recorded GenAI attributes, then application policy metadata. |
| 7 | 3 | Instrumentation, OTel providers, then OTLP to Aspire. |
| 8 | 3 | Evaluate, decide, enforce; then switch to the live demo. |
| 9 | 3 | Host-selected roles, policy decisions, then 0 versus 1 simulated tool entries. |
| 10 | 3 | Policy counts, run outcomes, then allowed versus successful. |
| 11 | 2 | Content handling, then outcomes and pipeline completeness. |
| 12 | 1 | Reveal the three takeaways. |

Slides 5 and 10 derive their charts from the checked-in eleven-run offline rehearsal
in `docs/evidence/session.json` (nine warm-ups plus two role checks). Their timestamps and tokens are synthetic demo
data, not production measurements. Current dashboard results can differ after
additional requests. Short waterfall spans have a minimum-width marker; printed
durations remain accurate. The export view shows every build at its final state.

## Before the audience arrives

1. Start Aspire, Python and Slidev using the README commands.
2. Keep slides, localhost:8090 and localhost:18988 in separate tabs.
3. Use offline mode for the timed demo. Click **Generate 9 offline demo runs** once.
   Warm-up always uses the standard-user profile and switches to offline mode.
   Expected deltas remain 6 allows, 3 denials, 3 failures and 1,398 synthetic tokens.
4. Check all three outcomes and execution counters. In Aspire choose resource
   governed-support-agent; clear earlier filters.
5. For optional real inference, select Foundry and rehearse a lookup first.
   Say the model is real, records are fictional, and first-tool selection is constrained.
6. Keep screenshots and backup slide 14 ready. Switch to offline mode or recorded
   evidence if a cloud dependency delays the talk.

Restart Python for clean counters. Aspire keeps its own in-memory session until
the container restarts; use a trace ID to locate a specific current run.

## Allowed — slide 8, three minutes

Keep the standard-user profile selected. Say: “The model requests a customer
lookup. Our host evaluates that request before the tool runs.” Click **Run allowed
lookup**. Explain the answer, matched allow rule, lookup-body counter of 1, and
deletion counter of 0.

Read the local tree: invoke_agent support-agent, first model call,
policy.evaluate lookup_customer, execute_tool lookup_customer, second model call.
All share one trace ID.

Click **Inspect traces in Aspire** to open this exact trace. You can also copy its
ID and add a `TraceId` filter. The enclosing HTTP request may appear above the agent span.
Select a model call and show its model/token attributes. Content is not captured.

## Role-gated deletion — slide 9, three minutes

Say: “The request and arguments stay the same; the caller context changes.”
Point out that the request card names the required Support admin role and
`customer:delete` permission before comparing the two caller profiles.
With **Standard user** selected, click **Run deletion request**. Point to
deny-non-admin-deletion and deletion-body count 0. The policy span is not ERROR,
and no tool execution span appears.

Select **Support admin** and click **Run deletion request** again. Point to
allow-admin-deletion, the execute_tool span and deletion-body count 1. Explain
that this is a safe simulated tool: no customer record is changed. The selector
represents two predefined demo profiles, not a real login or identity provider.

A missing span alone cannot prove non-execution. Here the same host boundary also
checks actual tool-body entry counters. Show the correlated deny log. An ordinary
retry as the standard user should not bypass the boundary.

## Failed — slide 10, three minutes

Say: “Permission does not guarantee success.” Select **Standard user** and click
**Run lookup with timeout**.
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
- Identity: the demo has predefined caller profiles only; it does not implement
  authentication or connect an identity provider.
- Approvals: backup slide 15 discusses binding approval to agent, tool, arguments
  and policy version. This demo does not implement or send approval requests.
- Standards: refer to sources; this pinned OpenLLMetry release does not imply
  support for every current GenAI draft field.

Keep durable auditing and evaluation to the production slide.
