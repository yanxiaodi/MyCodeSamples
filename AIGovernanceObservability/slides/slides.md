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
</div>
<HeroDiagram />
</div>
<div class="footer"><span>Python · one agent · real execution evidence</span><span>25 minutes</span></div>

<!--
The title graphic draws its two parallel lanes once; do not wait for it.

[00:00–00:30 | 30 seconds]
Today we are looking at two capabilities that belong beside each other: governance and observability.
Governance gives an agent a boundary for action. Observability gives us a way to understand a particular run and the behavior of the application over time.
We will use one small Python support agent throughout. The ideas apply just as well to an application written in another language. The goal is to leave with a practical way to connect a requested action, a policy decision, and the evidence of what executed.
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
Imagine that our support agent returns this message: “I couldn’t complete that request.” As the person using the application, you might reasonably ask it to try again. As the developer, you need to know something else first.
Did the agent request a destructive operation and encounter a policy boundary? Or did it request an ordinary lookup, receive permission, and then encounter a timeout?
Those cases should produce different operational responses. Retrying the destructive operation does not repair a policy denial. Tightening the policy does not repair a slow customer service.
The opposite is true as well: an agent saying “Done” does not independently establish what it did, whether a tool reached its destination, or whether the application applied the expected permission check.
So our starting point is the application behavior behind the answer. We want to understand the action request, the control applied to it, and the actual execution.
-->

---
class: paper-slide
---

# Two capabilities, one application

<div class="capability-columns">
<div class="capability-side"><svg viewBox="0 0 60 60" class="capability-icon" aria-hidden="true"><path d="M30 6 49 14v17c0 12-19 23-19 23S11 43 11 31V14Z"/><path d="m20 29 7 7 14-17"/></svg><h2>Governance</h2><div class="capability-question">May it act?</div><div class="capability-details"><span>Policy and permissions</span><span>Tool access and approval</span><span>Host enforcement</span></div></div>
<div class="capability-side"><svg viewBox="0 0 60 60" class="capability-icon" aria-hidden="true"><circle cx="26" cy="25" r="17"/><path d="m39 39 14 14 M16 29v-9 M26 32V16 M36 27V20"/></svg><h2>Observability</h2><div class="capability-question">What happened?</div><div class="capability-details"><span>Execution traces</span><span>Correlated logs</span><span>Latency, tokens and outcomes</span></div></div>
</div>
<div class="note">Identity, auditing and evaluation belong in the wider system. These two responsibilities work alongside each other.</div>
<div class="footer"><span>One application. Complementary responsibilities.</span><span>3 / 12</span></div>

<!--
The cool background is a visual reset. The two responsibilities have equal emphasis.

[02:00–03:30 | 90 seconds]
Governance asks whether an action is permitted. In an application, that means interpreting the policy, understanding the relevant context, and ensuring the host applies the result before execution.
Observability asks what happened. It gives us execution relationships, timings, outcomes and diagnostic context. We can inspect one run or look at patterns across many runs.
They are parallel capabilities. A trace does not stop a destructive operation. A policy decision does not explain a slow downstream dependency. We need both when the application can act.
There are other capabilities around an agent too. Identity establishes who is acting and on whose behalf. Auditing concerns the evidence we retain and the guarantees attached to it. Evaluation judges the quality of outputs and behavior. Each deserves its own treatment; this talk will not try to cover that entire landscape.
Our demo focuses on one boundary and one investigation: what happens when the model requests a tool.
-->

---
clicks: 3
---

# Control belongs at the action boundary

<AgentFlow scenario="compare" />

<div class="lesson">The model proposes. The policy decides. The host enforces.</div>
<div class="note">An unmatched action defaults to deny in this demo.</div>
<div class="footer"><span>Permission must be checked before the tool body.</span><span>4 / 12</span></div>

<!--
Build cues: click 1 evaluates the request; click 2 reveals the decision; click 3 shows the two host branches.

[03:30–05:00 | 90 seconds]
The model provides a proposed tool call: its name and arguments. The application validates that request and passes the action context to a policy engine.
For this demonstration, the policy engine is the official Agent Governance Toolkit. Our policy explicitly allows synthetic customer lookups and denies deletion. Unmatched actions default to deny.
The result is a decision. The host must then apply it. If the decision allows the action, execution can enter the tool body. If it denies the action, the tool body must not run.
This distinction is useful beyond a particular framework. Prompt instructions can influence what a model asks for, but the host boundary is where the application controls what actually happens.
We will also record the policy check separately from the tool execution. That allows us to distinguish “the model requested a tool,” “the policy allowed the request,” and “the tool executed.” Those are three different facts, and our evidence should preserve them.
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
Here is a representative run from our offline rehearsal. A trace connects operations belonging to one execution. A span describes one of those operations: when it started, how long it took, and the attributes or events we recorded about it.
The agent invocation contains a model call, a policy check and a tool execution. In the browser application there is also an outer HTTP request span. I have kept the agent operations on this slide so we can focus on the behavior.
We can immediately see that most of this run's time is in the tool body. The model call is much shorter. The policy check allowed the lookup and took less than a millisecond in this sample. The tool then encountered our injected timeout.
This is already a better question than “Why is the AI slow?” We can ask about the customer service, the timeout configuration, and whether the host handles the failure appropriately.
Logs complement the trace. A structured policy log can identify the matching rule. An error log can identify the timeout. A shared trace ID lets us associate them with this run, rather than search through unrelated messages.
Metrics answer a different question: how often does this occur? They aggregate many operations into counts and distributions. We might count policy decisions, measure tool duration, or examine input and output token usage.
The three signals are complementary. We use a metric to notice a pattern, a trace to locate a particular operation, and a log to explain the recorded context.
One limit matters: instrumentation records observable operations. A trace does not expose a model's private reasoning, and it does not automatically tell us whether the final answer is correct.
-->

---
clicks: 2
---

# A common language for AI telemetry

<div class="schema-grid">
<div class="schema-terminal">
<div class="schema-comment">// Recorded model-span attributes</div>
<div>{</div>
<div v-click="1"><span class="schema-key">gen_ai.operation.name</span>: <span class="schema-value">"chat"</span><br><span class="schema-key">gen_ai.request.model</span>: <span class="schema-value">"demo-model"</span><br><span class="schema-key">gen_ai.usage.input_tokens</span>: <span class="schema-value">80</span><br><span class="schema-key">gen_ai.usage.output_tokens</span>: <span class="schema-value">18</span></div>
<div>}</div>
<div v-click="2" class="schema-business"><div class="schema-comment">// Application policy metadata</div><span class="app-key">app.governance.decision</span>: "allow"</div>
</div>
<div class="schema-roles"><div class="schema-role"><strong>Conventions describe.</strong><span>Shared names and meanings.</span></div><div class="schema-role"><strong>Instrumentation collects.</strong><span>OpenLLMetry observes SDK operations.</span></div><div class="schema-role"><strong>OpenTelemetry exports.</strong><span>Context, providers and pipelines.</span></div><div class="schema-role"><strong>The dashboard visualizes.</strong><span>Queries, traces and metrics.</span></div></div>
</div>
<div class="note">GenAI conventions are in Development. Pin your libraries and verify their actual output.</div>
<div class="footer"><span>Use app.* for your own business metadata.</span><span>6 / 12</span></div>

<!--
Build cues: click 1 reveals the standard model fields; click 2 adds the application policy metadata.

[08:00–09:30 | 90 seconds]
If every library uses different attribute names for the same information, our queries and dashboards become tied to each implementation.
GenAI semantic conventions provide a shared vocabulary for model operations, agents, tools, metrics and events. These example attributes describe the operation, the model and token usage. They are useful across programming languages.
Conventions and instrumentation have different roles. A convention defines what the data means. OpenLLMetry provides instrumentations that collect model SDK activity. OpenTelemetry provides the SDKs, context and export mechanisms. The dashboard makes that data visible.
The official GenAI conventions are still in Development. Implementations may follow different revisions. For example, the OpenLLMetry release used here emits a token usage histogram, while newer draft conventions also define different inference token instruments. We should verify what a pinned library actually emits rather than promise that it implements every current draft field.
Our business-policy fields belong in an application namespace. We use app.governance.decision and app.governance.rule, rather than inventing new official gen_ai attributes.
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
This is the complete demonstration architecture. A small Python host runs a bounded tool-calling support agent. We instrument the OpenAI-compatible SDK with OpenLLMetry's individual OpenAI instrumentation. Our application records the policy boundary, tool execution and agent outcome.
We configure the OpenTelemetry trace, metric and log pipelines ourselves and export them over OTLP to a standalone Aspire dashboard. Aspire can receive this data from Python; the application does not need to be a .NET application. Traceloop SaaS is also optional.
Our offline mode still goes through the real SDK and instrumentation. Only its HTTP transport returns deterministic local responses. Its token counts are synthetic and clearly labelled. Foundry mode uses a configured cloud model and the usage reported by that model. A cloud failure does not silently become a successful mock run.
For a repeatable presentation, the first model turn is constrained to the tool for the selected scenario. The model still produces a native tool call; the host still validates and governs it. This isolates the behavior we want to demonstrate rather than depending on an unpredictable tool selection.
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
Switch to http://localhost:8090. Keep Offline demo selected for the primary presentation. If the network and model have been rehearsed, Foundry is an optional substitution.
Click Run allowed lookup. Read the response briefly: it contains the synthetic customer's support status. Point out one lookup execution and zero deletion executions.
Select the run. Follow the model call, policy check, tool execution and final model response. The model request is not the execution itself; the policy boundary sits between them.
Open Aspire in the other tab. Use the trace ID or the POST /api/run entry to find this run. Expand the agent operations, then select the policy span. Show allow and the matching rule. Select the model span and show model name and token usage. Do not enable prompt capture for this demonstration.
Show the correlated policy log. It belongs to the same trace, which is what makes it useful during an investigation.
If the segment finishes early, ask the room: which of these signals would you inspect first if the response was unexpectedly slow? Use their answer to point at the model and tool durations.
If the dashboard is unavailable, use the recorded span tree in the browser demo or the backup screenshots.
-->

---
clicks: 3
---

<div class="live"><span class="pill deny">Live demo · 2</span><a href="http://localhost:8090" target="_blank">Request denied deletion</a></div>

# Blocked: the control worked

<div class="demo-request">“Delete customer record C-1042.”</div>
<AgentFlow scenario="blocked" />
<div class="execution-proof"><span class="proof-count deny">0</span><div class="proof-label">Deletion tool body entries<br><span class="note">Rule: block-destructive</span></div><div class="proof-detail">The policy span is not ERROR. Verify the execution counter; a missing span alone is insufficient.</div></div>
<div class="footer"><span>Policy deny. Agent blocked. No tool execution.</span><span>9 / 12</span></div>

<!--
Build cues: use the three clicks to explain evaluation, deny and the host stop. The zero is a recorded execution counter.

[14:00–17:00 | 3-minute live segment]
Click Request denied deletion. The first model turn requests delete_record. The policy engine matches block-destructive. The host returns the blocked result before entering the tool body.
Show the two separate facts: a deny decision and zero deletion body executions. The mock deletion function is a tripwire rather than a destructive function, so even a broken control cannot delete real data in this demonstration.
Open this trace in Aspire. It contains the model call and the policy check, but no executed deletion tool span. Select the policy span and show its rule and decision attributes. Then show the correlated policy log.
Notice that the policy span need not be marked as an error: evaluating the rule and returning deny is the intended behavior. The agent outcome is blocked. This is different from a dependency failure, which we will look at next.
A missing span by itself is weak evidence. Sampling, export failure or incorrect instrumentation can also make a span disappear. Our execution counter complements the trace by recording whether the tool body was entered.
Return to the overview and point out the blocked policy counter. An increasing denial count can mean controls are working, requests changed, or policy is misconfigured. It needs context before it becomes an alert.
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
Build cues: click 1 shows 6 allow and 3 deny; click 2 shows 3 succeeded, 3 blocked and 3 failed; click 3 emphasizes their difference. These charts use the saved nine-run offline rehearsal, not live or production data.

[17:00–20:00 | 3-minute live segment]
Click Run lookup with timeout. The request is identical to the allowed scenario; the scenario injects an approximately 800-millisecond wait followed by a timeout in the tool. This is a controlled simulation, not a real dependency outage.
Show that the policy decision remains allow. The lookup body was entered once. It then failed. That distinction is preserved in both the trace and the aggregate metrics.
Open the trace. The model call is short, the policy evaluation is short, and the tool execution consumes most of the duration. Select the tool span and show error.type: TimeoutError. The agent root is also a failed run, while the policy span records a successful evaluation that allowed the action.
Show the error log with the same trace ID. We can now investigate the downstream operation instead of treating this as a generic AI failure.
Use the overview's warm-up button if it has not already been run. It generates nine labelled offline runs. Point out the four counters and the duration histograms. The token total comes from OpenLLMetry's metric, and the application supplies policy decisions and run outcomes.
Do not describe these few samples as production percentiles. Their purpose is to demonstrate how aggregate signals and individual execution evidence work together.
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
There are four practical lessons I would carry into a production application.
First, preserve correlation. A single-process demonstration inherits trace context easily. A real application crosses HTTP calls, queues, background jobs and sometimes an approval step that resumes in another request. Context propagation must be deliberate. A conversation identifier can connect related requests, but it is not a substitute for preserving the trace relationships of a particular operation.
Second, decide what content to capture. You can learn a great deal from operation names, model identifiers, durations, token usage and error types without collecting entire prompts, completions or customer records. Our default is content capture off. Turning it on changes the data handling requirements of the observability system.
Third, keep outcomes separate. A denied action may represent a successful control. A permitted action may fail operationally. Token counts help explain resource usage, but a token count is not itself a price. Turning usage into cost requires the relevant model and pricing context. Nor do a fast trace and low token count prove answer quality; evaluation answers that different question.
Fourth, verify the complete telemetry pipeline. Seeing traces does not prove you are exporting metrics or logs. The actual behavior depends on the SDK version, instrumentation, providers and destination configuration. Pin versions and check what arrives. Avoid instrumenting the same model call twice, especially when an agent framework already has instrumentation.
Finally, diagnostic telemetry and audit evidence have different guarantees. Sampling or retention may remove diagnostic records. If you need complete, durable audit evidence, define that pipeline explicitly. Aspire is useful for local diagnostics; long-term production storage and access should be designed around your requirements.
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
The useful pattern is small. Enforce a policy before a tool can act. Record the decision and the execution as separate facts. Connect those facts to the model and the rest of the application with trace context.
Then use the signals for the questions they answer best: metrics for patterns, traces for individual execution, and logs for recorded context.
Shared conventions make this portable across languages and providers. They still need a real, verified implementation. Today we saw the pattern in Python with AGT, OpenLLMetry, OpenTelemetry and Aspire. The same control and evidence boundary can be implemented in another stack.
When you next add a tool to an agent, ask two questions together: is the agent allowed to do this, and how will we know what actually happened?
Pause for questions. If asked about approvals, cross-service propagation or standards changes, use the backup slides. If asked about identity, audit guarantees or evaluation, identify their role without presenting the demo as a complete production governance program.
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
<div class="note">Recorded offline rehearsal. All customer records and model usage counts are synthetic; the telemetry is collected through the real SDK and OpenLLMetry.</div>
<div class="footer"><span>Use if a live tab or dashboard is unavailable.</span><span>Backup</span></div>

<!--
This image is captured from the browser demonstration after nine offline runs. It is a fallback, not a mockup of the intended telemetry. Prefer the live application so the audience can follow one trace and observe the counters change.
-->

---

# Backup: approvals and standards

<div class="two"><div><h2>Human approval</h2><p>A policy can require approval.<br>The host pauses and resumes execution.</p><p>Approval must be bound to the action and its arguments.</p><p class="note">A resumed request may have a new trace. Preserve the relationship deliberately.</p></div><div><h2>Conventions evolve</h2><p>Inspect actual emitted fields and units.</p><p>Use <code>app.*</code> for business policy metadata.</p><p>Trace export does not automatically configure metric export.</p><p class="note">The newer GenAI draft and a pinned instrumentation may use different metric names.</p></div></div>
<div class="footer"><span>Useful extensions beyond the three live scenarios</span><span>Backup</span></div>

<!--
Approval is deliberately omitted from the primary demonstration so we have time to investigate all three outcomes. The earlier governance talk provides an approval example. In production, an approval lifecycle needs action binding, expiration and correlation across resume boundaries.
OpenLLMetry supports the Responses API too. In the inspected version, its Responses tracing and automatic metric paths are not identical to Chat Completions, so we selected the latter for this presentation and verified the signals that arrive.
-->
