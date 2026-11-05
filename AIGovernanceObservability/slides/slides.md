---
theme: default
title: AI Agent Governance and Observability in Practice
author: Xiaodi Yan
info: |
  A 25-minute English talk with a Python, Agent Governance Toolkit,
  OpenLLMetry and OpenTelemetry demonstration.
duration: 25min
aspectRatio: 16/9
canvasWidth: 960
transition: slide-left
colorSchema: dark
fonts:
  sans: Aptos
  mono: Cascadia Code
  provider: none
drawings:
  persist: false
layout: default
class: opening-slide
---

<div class="opening-grid">
<div class="opening-copy">
<h1>AI Agent Governance<br>and Observability<br>in Practice</h1>
<div class="opening-lines"><span>Control what the agent can do.</span><span>Understand what actually happened.</span></div>
<div class="speaker">Xiaodi Yan · Microsoft MVP</div>
<img class="speaker-mvp-badge" src="/branding/mvp-badge-full-color-blue-black.png" alt="Microsoft Most Valuable Professional (MVP)" width="1440" height="540" />
</div>
<HeroDiagram />
</div>
<div class="footer"><span>Python · one agent · real execution evidence</span><span>25 minutes</span></div>

<!--
The title graphic draws its two parallel lanes once; do not wait for it.

[00:00–00:30 | 30 seconds]
Today we are looking at two capabilities that belong together: governance and observability. Governance gives an agent a boundary for action; observability helps us understand a run and the behavior of the application over time. We will use a small Python support agent to connect a requested action, a policy decision, and evidence of what actually executed.
-->

---
clicks: 2
---

# Would you retry this?

<div class="response-quote">“I couldn’t complete<br>that request.”</div>
<div class="response-caption">The agent’s final response</div>
<div class="response-forks">
<div v-click="1" class="fork-node"><h2 class="deny">The action was blocked.</h2><p>The policy may have worked exactly as intended.</p></div>
<div v-click="2" class="fork-node failed-fork"><h2 class="fail">The action failed.</h2><p>The policy may have allowed it; a dependency timed out.</p></div>
</div>
<div class="footer"><span>The same answer can hide two very different runs.</span><span>2 / 12</span></div>

<!--
Build cues: Ask the audience whether they would retry. Click once to reveal the blocked action, then again for the failed action.

[00:30–02:00 | 90 seconds]
Imagine our support agent returns this message. You might reasonably ask it to try again, but first we need to know what happened. Did the agent hit an intended policy boundary, or did an allowed lookup time out? These cases need different responses: retrying does not fix a policy denial, and tightening policy does not fix a slow dependency. And “Done” does not prove what the tool did or whether the application applied the expected permission check.
-->

---
class: paper-slide
---

# Two capabilities, one application

<div class="capability-columns">
<div class="capability-side"><svg viewBox="0 0 60 60" class="capability-icon" aria-hidden="true"><path d="M30 6 49 14v17c0 12-19 23-19 23S11 43 11 31V14Z"/><path d="m20 29 7 7 14-17"/></svg><h2>Governance</h2><div class="capability-question">May it act?</div><div class="capability-details"><span>Policy and permissions</span><span>Tool access and approval</span><span>Host enforcement</span></div></div>
<div class="capability-side"><svg viewBox="0 0 60 60" class="capability-icon" aria-hidden="true"><circle cx="26" cy="25" r="17"/><path d="m39 39 14 14 M16 29v-9 M26 32V16 M36 27V20"/></svg><h2>Observability</h2><div class="capability-question">What happened?</div><div class="capability-details"><span>Execution traces</span><span>Correlated logs</span><span>Latency, tokens and outcomes</span></div></div>
</div>
<div class="note">Caller roles are simulated to show authorization context. Login, identity verification, durable auditing and evaluation remain outside this demo.</div>
<div class="footer"><span>One application. Complementary responsibilities.</span><span>3 / 12</span></div>

<!--
The cool background is a visual reset. The two responsibilities have equal emphasis.

[02:00–03:30 | 90 seconds]
Governance asks whether an action is permitted; observability asks what happened through execution relationships, timings, outcomes, and context. They are complementary: a trace does not stop a destructive operation, and a policy decision does not explain a slow dependency. Identity, durable auditing, and evaluation matter too, but are outside this talk's scope. Here, two predefined caller roles simulate authorization context—they do not authenticate anyone. We will focus on what happens when the model requests a tool.
-->

---
clicks: 3
---

# Control belongs at the action boundary

<AgentFlow scenario="compare" />
<div class="policy-context"><div><code>lookup_customer</code><span><strong>customer:read</strong> · both roles</span></div><div><code>delete_record</code><span><strong>Support admin</strong> + <code>customer:delete</code></span></div></div>

<div class="lesson">The model proposes. The policy decides. The host enforces.</div>
<div class="note">The host supplies the demo caller role; the model does not choose it. Unmatched actions default to deny.</div>
<div class="footer"><span>Permission must be checked before the tool body.</span><span>4 / 12</span></div>

<!--
Build cues: click 1 evaluates the action and host-supplied role/permissions; click 2 reveals the decision; click 3 shows the host branches.

[03:30–05:00 | 90 seconds]
The model proposes a tool call; the application validates its name and arguments, then sends the action and host-selected simulated caller context to AGT. Lookup requires customer:read, available to both profiles; deletion requires the Support admin role and customer:delete. Unmatched actions default to deny. The host must enforce the result before the tool body: allow means execution may proceed; deny means it must not run. We record the policy check separately from execution—requested, allowed, and executed are three different facts. Prompting can influence requests, but the host boundary controls what actually happens.
-->

---
clicks: 4
---

# Where did the time go?

<TraceWaterfall scenario="failed" />

<div class="signal-strip"><div><strong>Trace</strong><span>Locate the slow operation.</span></div><div><strong>Log</strong><span>Read the rule and error context.</span></div><div><strong>Metrics</strong><span>Find the pattern across runs.</span></div></div>
<div class="footer"><span>Execution evidence, not the model’s private reasoning.</span><span>5 / 12</span></div>

<!--
Build cues: click 1 reveals the model call; click 2 the policy; click 3 the tool; click 4 emphasizes the tool share of this recorded trace.

[05:00–08:00 | 3 minutes]
Here is a representative run from our offline rehearsal. A trace connects operations in one execution; each span records timing and selected attributes. This run includes a model call, policy check, and tool execution; most of the delay is in our injected timeout. The trace helps locate the operation, logs identify the rule and error, and metrics show whether the pattern repeats. We use metrics to notice a pattern, traces to locate a run, and logs to explain its context. These signals describe observable behavior; they do not reveal private reasoning or establish answer quality.
-->

---
clicks: 2
---

# A common language for AI telemetry

<div class="schema-grid">
<div class="schema-terminal">
<div class="schema-comment">// Recorded model-span attributes</div>
<div>{</div>
<div v-click="1" class="schema-properties"><span class="schema-key">gen_ai.operation.name</span>: <span class="schema-value">"chat"</span><br><span class="schema-key">gen_ai.request.model</span>: <span class="schema-value">"demo-model"</span><br><span class="schema-key">gen_ai.usage.input_tokens</span>: <span class="schema-value">80</span><br><span class="schema-key">gen_ai.usage.output_tokens</span>: <span class="schema-value">18</span></div>
<div>}</div>
<div v-click="2" class="schema-business"><div class="schema-comment">// Application policy metadata</div><span class="app-key">app.governance.decision</span>: "allow"</div>
</div>
<div class="schema-roles"><div class="schema-role"><strong>Conventions describe.</strong><span>Shared names and meanings.</span></div><div class="schema-role"><strong>Instrumentation collects.</strong><span>OpenLLMetry observes SDK operations.</span></div><div class="schema-role"><strong>OpenTelemetry exports.</strong><span>Context, providers and pipelines.</span></div><div class="schema-role"><strong>The dashboard visualizes.</strong><span>Queries, traces and metrics.</span></div></div>
</div>
<div class="note">GenAI conventions are in Development. Pin your libraries and verify their actual output.<br>Project: <a href="https://github.com/open-telemetry/semantic-conventions-genai" target="_blank" rel="noopener noreferrer">https://github.com/open-telemetry/semantic-conventions-genai</a></div>
<div class="footer"><span>Use app.* for your own business metadata.</span><span>6 / 12</span></div>

<!--
Build cues: click 1 reveals the standard model fields; click 2 adds the application policy metadata.

[08:00–09:30 | 90 seconds]
If every library uses different names for the same information, queries and dashboards become tied to each implementation. GenAI semantic conventions provide shared names for model operations, tools, and token usage. Conventions describe what data means; OpenLLMetry instruments SDK activity, OpenTelemetry provides context and export, and the dashboard visualizes it. These conventions are still evolving, so check what your pinned versions actually emit. Put application policy fields under app.governance.* rather than inventing new gen_ai attributes.
-->

---
clicks: 3
---

# Instrument the model. Record the boundary.

<TelemetryMap />

<div class="implementation-strip"><code>os.environ["TRACELOOP_TRACE_CONTENT"] = "false"<br>OpenAIInstrumentor().instrument(<br>&nbsp;&nbsp;tracer_provider=traces, meter_provider=meters)</code><p>Own your OTel pipeline.<br>One instrumentation owner per model call.<br>Traceloop SaaS is optional.</p></div>
<div class="footer"><span>Python → OTLP → Aspire standalone</span><span>7 / 12</span></div>

<!--
Build cues: click 1 adds instrumentation; click 2 the OTel providers; click 3 exports to the dashboard.

[09:30–11:00 | 90 seconds]
This is the complete demo architecture: a small Python host runs the support agent, and OpenLLMetry instruments the OpenAI-compatible SDK. Our application records the policy boundary, tool execution, and agent outcome; OpenTelemetry trace, metric, and log pipelines export to the standalone Aspire dashboard over OTLP. Aspire can receive Python telemetry without a .NET app, and Traceloop SaaS is optional. Offline mode still uses the real SDK and instrumentation, but returns deterministic responses and labelled synthetic token counts; cloud failures are not silently turned into mock successes. To keep the demo repeatable, the first model turn is constrained to the selected tool while the host still validates and governs it.
-->

---
clicks: 3
---

<div class="live"><span class="pill allow">Live demo · 1</span><a href="http://localhost:8090" target="_blank">Open the support agent</a></div>

# Allowed: the tool executes

<div class="demo-request">“Look up customer C-1042 and summarize their support status.”</div>
<AgentFlow scenario="allowed" />
<div class="execution-proof"><span class="proof-count allow">1</span><div class="proof-label">Lookup tool body entry<br><span class="note">Deletion entries: 0</span></div><div class="proof-detail">Two model calls: request the tool, then compose the answer. Follow the shared trace ID.</div></div>
<div class="footer"><span>Model request → allow → execution → answer</span><span>8 / 12</span></div>

<!--
Build cues: use the three clicks to explain policy evaluation, decision and host execution before switching to the live tab.

[11:00–14:00 | 3-minute live segment]
Switch to http://localhost:8090 and select Standard user; keep Offline demo selected for the primary presentation. Foundry is optional if you have rehearsed it. Run the allowed lookup and briefly show the synthetic support status, lookup-body counter of 1, and deletion counter of 0. Follow the model request, policy check, tool execution, and final model response: the request is not execution; the policy boundary sits between them. In Aspire, find the run by trace ID, show the allow rule, model/token attributes, and correlated log. Keep prompt capture off; if the dashboard is unavailable, use recorded evidence. If time permits, ask which signal they would inspect first if the response were slow.
-->

---
clicks: 3
---

<div class="live"><span class="pill deny">Live demo · 2</span><a href="http://localhost:8090" target="_blank">Compare simulated caller roles</a></div>

# Same action, different roles

<div class="demo-request"><div>“Delete customer record C-1042.”</div><div class="required">Required: <strong>Support admin</strong> role + <code>customer:delete</code> permission</div></div>
<RoleComparison />
<div class="note">Predefined demo profiles; no login or identity provider. The request and tool arguments stay the same.</div>
<div class="footer"><span>Host-supplied caller context changes the decision. Deletion remains simulated.</span><span>9 / 12</span></div>

<!--
Build cues: click 1 reveals the host-supplied caller profiles; click 2 their policy decisions and rules; click 3 the recorded 0 versus 1 deletion-body entries.

[14:00–17:00 | 3-minute live segment]
Select Standard user and run the deletion request; AGT matches deny-non-admin-deletion and the host stops before the tool body. Then switch to Support admin and submit the same request; allow-admin-deletion runs the safe simulated tool once, changing no record. Point out the required Support admin role and customer:delete permission: only the host-supplied caller context changes. Compare the traces, rules, correlated logs, and execution counter: zero body entries for Standard user, one simulated entry for Support admin. The profiles are predefined, not real logins; a denial is an intended control outcome, not an error. The counter complements the trace: a missing span alone cannot prove non-execution.
-->

---
clicks: 3
---

<div class="live"><span class="pill fail">Live demo · 3</span><a href="http://localhost:8090" target="_blank">Run lookup with timeout</a></div>

# Allowed does not mean successful

<div class="failure-grid">
<div class="failure-route"><div class="failure-state allow">Allowed<small>Policy decision</small></div><div class="state-link"></div><div class="failure-state">Entered<small>Lookup body: 1</small></div><div class="state-link"></div><div class="failure-state">Timed out<small>Injected 800 ms tool timeout</small></div></div>
<OutcomeDashboard />
</div>
<div class="footer"><span>Metrics reveal patterns. Traces locate them. Logs explain them.</span><span>10 / 12</span></div>

<!--
Build cues: click 1 shows 7 allow and 4 deny; click 2 shows 4 succeeded, 4 blocked and 3 failed; click 3 compares permission with success. The chart includes all eleven saved offline runs: nine warm-ups plus two role checks.

[17:00–20:00 | 3-minute live segment]
Select Standard user and run lookup with timeout. This is the same allowed lookup, with a controlled ~800 ms wait and timeout—not a real dependency outage. Policy is allow, the lookup body is entered once, and the run then fails; trace and metrics preserve that distinction. In Aspire, show the TimeoutError tool span, the successful policy span, and the correlated error log. The warm-up adds nine labelled offline runs; point out the counters and duration histograms, but do not describe these samples as production percentiles. The slide chart includes eleven saved runs: the nine warm-ups plus two role checks.
-->

---
class: paper-slide
clicks: 2
---

# Make the evidence useful in production

<div class="production-grid">
<div class="production-item"><svg class="production-icon" viewBox="0 0 40 40" aria-hidden="true"><rect x="3" y="4" width="10" height="10" rx="2"/><rect x="27" y="26" width="10" height="10" rx="2"/><path d="M13 9h19v17 M8 14v17h19"/></svg><div><h2>Keep correlation intact.</h2><p>Preserve context across tools and services.<br>Connect each policy log to its run.</p></div></div>
<div v-click="1" class="production-item"><svg class="production-icon" viewBox="0 0 40 40" aria-hidden="true"><rect x="4" y="5" width="32" height="29" rx="3"/><path d="M10 13h20 M10 20h12 M10 27h20" stroke-dasharray="3 2"/></svg><div><h2>Capture content deliberately.</h2><p>Start with model, duration and token usage.<br>Handle prompt and result capture deliberately.</p></div></div>
<div v-click="2" class="production-item"><svg class="production-icon" viewBox="0 0 40 40" aria-hidden="true"><path d="M20 5v29 M6 12h28 M6 12 1 25h10Z M34 12 29 25h10Z M10 35h20"/></svg><div><h2>Keep outcomes separate.</h2><p>Denied action ≠ failed execution.<br>Token counts ≠ cost or response quality.</p></div></div>
<div v-click="2" class="production-item"><svg class="production-icon" viewBox="0 0 40 40" aria-hidden="true"><circle cx="20" cy="20" r="16"/><path d="m10 20 7 7 14-16"/></svg><div><h2>Verify all three pipelines.</h2><p>Export traces, logs and metrics.<br>Design durable audit evidence separately.</p></div></div>
</div>
<div class="note">Aspire standalone is a diagnostic viewer. Production storage, retention and access controls need their own design.</div>
<div class="footer"><span>Instrument what supports an operational decision.</span><span>11 / 12</span></div>

<!--
Build cues: click 1 adds content handling; click 2 adds outcome separation and pipeline verification.

[20:00–23:00 | 3 minutes]
There are four practical lessons for production. First, preserve trace context across services, queues, and resumed work; a conversation ID is not a substitute for operation-level correlation. Second, capture content deliberately: model, duration, token, and error data may be useful without collecting prompts or customer records. Third, keep outcomes separate: a denial can mean the control worked, an allowed action can still fail, and token usage is neither cost nor answer quality. Fourth, verify that traces, logs, and metrics all reach their destinations, and avoid duplicate instrumentation. Diagnostic telemetry and durable audit evidence have different guarantees, so design storage, retention, and access for auditing separately.
-->

---
clicks: 1
---

<div class="closing-grid"><div><h1>Control<br>the action.</h1><p>Policy before execution.</p></div><div><h1>Understand<br>the run.</h1><p>Evidence connected by context.</p></div></div>
<div v-click="1" class="closing-principles"><span><b>Enforce the boundary.</b>Requested ≠ executed.</span><span><b>Connect the signals.</b>Traces, logs and metrics.</span><span><b>Verify the schema.</b>Shared conventions; actual output.</span></div>
<div class="footer"><span><a href="http://localhost:8090" target="_blank">Demo</a> · <a href="http://localhost:18988" target="_blank">Dashboard</a> · Questions</span><span>12 / 12</span></div>

<!--
Click once to show the three practical takeaways, then pause for questions.

[23:00–25:00 | closing and questions]
The useful pattern is simple: enforce policy before execution, record decision and execution separately, and connect them with trace context. Use metrics for patterns, traces for individual runs, and logs for recorded context. Shared conventions help portability, but verify what the implementation actually emits. When adding a tool, ask: is it allowed, and how will we know what happened? Pause for questions; use the backup slides for approval or standards topics, and clarify that the caller roles are simulated.
-->

---

# Sources and implementation notes

<ul class="sources">
<li><a href="https://github.com/microsoft/agent-governance-toolkit">Microsoft Agent Governance Toolkit</a> — runtime policy engine and host enforcement.</li>
<li><a href="https://github.com/traceloop/openllmetry">OpenLLMetry</a> — Python instrumentations; a separate JS/TS project is also available.</li>
<li><a href="https://github.com/open-telemetry/semantic-conventions-genai">OpenTelemetry GenAI semantic conventions</a> — spans, metrics, events and provider conventions.</li>
<li><a href="https://opentelemetry.io/docs/languages/python/">OpenTelemetry Python</a> — trace, metric and log pipelines.</li>
<li><a href="https://aspire.dev/dashboard/standalone/">Aspire standalone dashboard</a> — OTLP viewer for Python and other languages.</li>
</ul>
<div class="note">Pinned versions and source checks are in the repository. This demo uses Chat Completions to exercise both automatic model tracing and metrics.</div>
<div class="footer"><span>Backup material</span><span>References</span></div>

<!--
Backup only; not part of the 25-minute timed sequence.
The research note links immutable source revisions where useful. The installed Python packages are recorded in requirements.lock.txt. The dashboard image is pinned by digest. The new GenAI repository is still evolving, so use the package versions and actual exported fields together when reproducing the demonstration.
-->

---

# Backup: the actual demonstration

<img src="/demo-overview.png" class="screenshot" alt="Actual offline rehearsal dashboard showing policy decisions, runtime failures, tokens and execution evidence" />
<div class="note">Earlier nine-run rehearsal, before caller profiles. Use the live app for the role-aware flow. Records and model usage are synthetic; SDK telemetry is real.</div>
<div class="footer"><span>Use if a live tab or dashboard is unavailable.</span><span>Backup</span></div>

<!--
This image records the earlier nine-run browser rehearsal before caller profiles were added. The role comparison and eleven-run chart use the newer session.json from main. Prefer the live application so the audience can follow one trace and observe the counters change.
-->

---

# Backup: approvals and standards

<div class="two"><div><h2>Human approval</h2><p>A policy can require approval.<br>The host pauses and resumes execution.</p><p>Approval must be bound to the action and its arguments.</p><p class="note">A resumed request may have a new trace. Preserve the relationship deliberately.</p></div><div><h2>Conventions evolve</h2><p>Inspect actual emitted fields and units.</p><p>Use <code>app.*</code> for business policy metadata.</p><p>Trace export does not automatically configure metric export.</p><p class="note">The newer GenAI draft and a pinned instrumentation may use different metric names.</p></div></div>
<div class="footer"><span>Useful extensions beyond the three live scenarios</span><span>Backup</span></div>

<!--
Approval is deliberately omitted from the primary demonstration so we have time to investigate all three outcomes. The earlier governance talk provides an approval example. In production, an approval lifecycle needs action binding, expiration and correlation across resume boundaries.
OpenLLMetry supports the Responses API too. In the inspected version, its Responses tracing and automatic metric paths are not identical to Chat Completions, so we selected the latter for this presentation and verified the signals that arrive.
-->
