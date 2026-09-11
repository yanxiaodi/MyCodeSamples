---
theme: default
title: "Governing AI Agents: From Autonomy to Accountability"
info: |
  A 20-minute meetup talk about the engineering surface around autonomous AI agents.
author: Xiaodi Yan
keywords: AI agents, governance, guardrails, security, audit, observability
transition: fade
duration: 20min
aspectRatio: 16/9
canvasWidth: 960
fonts:
  sans: Inter
  mono: JetBrains Mono
class: deck
---

<div class="hero-mark">AGENT / GOVERNANCE</div>

# Governing AI Agents

## From autonomy to accountability

<div class="hero-rule"></div>

<p class="lede">Building agents that can act — and still be trusted.</p>

<div class="speaker-line">Xiaodi Yan · Lead Engineer, Kiwibank · Microsoft MVP</div>
<div class="event-line">AI + Dev Meetup · mate.dev & Wellington .NET User Group · 10 September 2026</div>
<img class="mvp-badge" src="/mvp-badge-blue-black.png" alt="Microsoft MVP" width="165" height="62" style="position:absolute;right:24px;bottom:16px;width:165px !important;height:62px !important;max-width:none !important;display:block;" />

<!--
Hi everyone, my name is Xiaodi, and I am a Lead Engineer at Kiwibank and a Microsoft MVP. I organise Wellington .NET User Group so I am very excited to be here today to talk about governing AI agents.

How many of you have used AI agents before? like ChatGPT, Claude Code, or GitHub Copilot? Raise your hand if you have.
If you have, you know that these agents can do more than just provide answers — they can take actions on your behalf.
But sometimes you need to be careful about what actions they take, as they can delete your files or perform other destructive operations.
That's why almost all these agents ask for confirmation before performing actions that have significant side effects.

That creates a new engineering question. It is not enough to ask whether the model can produce a good answer. We also need to ask: what is the agent allowed to do, who did it, and can we prove what happened?

Today I want to explore those questions, and then show one practical way to enforce governance at runtime.
-->

---

<div class="eyebrow">01 / THE SCENARIO</div>

# The agent that can act

<p class="lede">Imagine a customer-support agent with access to real tools.</p>

<div class="tool-grid four">
  <div class="tool-card safe"><span class="tool-code">READ</span><strong>lookup_customer</strong><small>Customer context</small></div>
  <div class="tool-card caution"><span class="tool-code">MONEY</span><strong>issue_refund</strong><small>Financial side effect</small></div>
  <div class="tool-card caution"><span class="tool-code">SEND</span><strong>send_email</strong><small>External communication</small></div>
  <div class="tool-card danger"><span class="tool-code">DELETE</span><strong>delete_record</strong><small>Destructive operation</small></div>
</div>

<div class="question-strip">The moment tools have side effects, this is no longer “just a chatbot”.</div>

<!--
Imagine that we are building a customer-support agent.

It can look up a customer, issue a refund, send an email, and update or delete a record. The model is responsible for understanding the customer’s request and choosing the appropriate tool.

At first, this looks like a normal AI application. We have a prompt, a model, and a few functions.

But because those functions have real side effects, the application becomes more than a chatbot.

So let’s say the customer asks: “Please remove my old account information.” Should the agent be allowed to do that automatically? Should it require approval? And what happens if the model misunderstands the request?
-->

---

<div class="eyebrow">02 / THE BASELINE DEMO</div>

# A support agent without governance

<p class="lede">The model chooses a tool, and the host executes it.</p>

<div class="demo-architecture">
  <div class="provider primary-provider">AZURE AI FOUNDRY<br><small>model provider</small></div>
  <div class="demo-arrow">→</div>
  <div class="demo-agent">AGENT<br><small>one action at a time</small></div>
  <div class="demo-arrow">→</div>
  <div class="ungoverned-boundary">DIRECT<br>TOOL ACCESS</div>
</div>

<div class="demo-tools"><span class="allow">lookup_customer</span><span class="warn">send_email</span><span class="deny">delete_record</span></div>

<div class="demo-note">Baseline only · mock customer records · controlled test mailbox · no policy layer</div>

<div class="question-strip">The agent can act, but the host has no shared answer for what it may do.</div>

<!--
Before we add governance, let’s run the same support agent in its baseline form.

The model receives a customer request, chooses a tool, and returns a tool call. The host passes that call directly to the function. There is no policy decision between the agent and the tool.

I’ll start with a customer lookup. The read works as expected. Then I’ll ask the agent to send an email, and finally to delete a record. The model can request all three actions because the host exposes all three tools directly.

This demo stays inside mock customer records and a controlled test mailbox. The point is to see the execution path: the model makes the decision, and the host executes it.

That path is simple, but it leaves important questions unanswered. Which actions should run automatically? Which ones need approval? And how would we prove what happened later?
-->

---

<div class="eyebrow">03 / THE QUESTIONS</div>

# Every agent action raises three questions

<div class="question-grid">
  <div class="big-question"><div class="question-number">01</div><strong>Is this action allowed?</strong><span>Policy, scope, approval</span></div>
  <div class="big-question"><div class="question-number">02</div><strong>Who performed it?</strong><span>Identity, session, delegation</span></div>
  <div class="big-question"><div class="question-number">03</div><strong>Can we prove what happened?</strong><span>Telemetry, audit, evidence</span></div>
</div>

<div class="bottom-line">These questions are the bridge between autonomy and accountability.</div>

<!--
Every time an agent tries to use a tool, we should be able to answer three questions.

First: is this action allowed? An agent may have access to a service, but that does not mean every operation in that service should be permitted.

Second: who performed this action? Was it the user, the agent, another delegated agent, or a shared service identity?

Third: can we prove what happened? What did the agent request? What tool was called? Was the action allowed, denied, transformed, or escalated?

These questions are the bridge between autonomy and accountability.
-->

---

<div class="eyebrow">04 / THE SYSTEM</div>

# An agent is a production system

<div class="system-map">
  <div class="map-row map-top"><span class="map-node">USER</span><span class="map-arrow">→</span><span class="map-node primary">AGENT</span><span class="map-arrow">→</span><span class="map-node">MODEL</span></div>
  <div class="map-branch"><span class="map-node">TOOLS / APIs</span><span class="map-node">DATA</span><span class="map-node">OTHER AGENTS</span></div>
  <div class="map-effect">REAL-WORLD EFFECTS</div>
</div>

<div class="control-row">
  <span>IDENTITY</span><span>POLICY</span><span>ISOLATION</span><span>OBSERVABILITY</span><span>AUDITABILITY</span><span>RELIABILITY</span>
</div>

<p class="lede compact">The model is one component. The risk appears at the action boundary.</p>

<!--
One mistake we can make is to think of an agent as just a prompt plus a model.

In production, an agent is a much larger system. It includes the model, but also tools, APIs, databases, user identities, delegated agents, operational controls, and sometimes external side effects.

The model is only one component in this system. The risk often appears at the boundary between the model and the tools it can call.

That is why building an agent is not just a coding exercise. We also need identity, policy enforcement, isolation, observability, auditability, and reliability engineering.

The more autonomy we give the agent, the more important these surrounding controls become.
-->

---

<div class="eyebrow">05 / THE VOCABULARY</div>

# Governance is bigger than a guardrail

<div class="two-panel">
  <div class="panel governance-panel">
    <div class="panel-label">GOVERNANCE ASKS</div>
    <ul>
      <li>What should be allowed?</li>
      <li>Who is responsible?</li>
      <li>What evidence is required?</li>
    </ul>
  </div>
  <div class="panel guardrail-panel">
    <div class="panel-label">GUARDRAILS ENFORCE</div>
    <div class="verdicts"><span>ALLOW</span><span>WARN</span><span>DENY</span><span>ESCALATE</span><span>TRANSFORM</span></div>
  </div>
</div>

<div class="micro-grid"><span>IDENTITY → who</span><span>O11Y → what happened</span><span>AUDITABILITY → what we can prove</span><span>SECURITY → what is protected</span><span>RELIABILITY → how failures are controlled</span></div>

<!--
Today we're talking about AI agent governance.
You may also hear the term "guardrails" in this context.
It is useful to distinguish governance from guardrails.

Governance is the broader system. It defines what the agent is allowed to do, who is responsible for those decisions, and what evidence needs to be retained.

A guardrail is one way to enforce part of that governance at runtime. It can allow an action, warn about it, deny it, escalate it for approval, or transform it before execution.

Identity tells us who is acting. Observability tells us what happened. Audit provides evidence. Security protects the boundaries, and reliability engineering helps us control failures.

So governance is not simply “adding a filter to the prompt”. It is a set of controls around the agent’s ability to act.
-->

---

<div class="eyebrow">06 / THE RISK MAP</div>

# OWASP Agentic Top 10: a risk vocabulary

<div class="risk-grid">
  <div class="risk-card focus"><b>ASI01 · GOAL HIJACK</b><span>Attacker changes what the agent pursues.</span></div>
  <div class="risk-card focus"><b>ASI02 · TOOL MISUSE & EXPLOITATION</b><span>Legitimate tools are used unsafely.</span></div>
  <div class="risk-card focus"><b>ASI03 · IDENTITY & PRIVILEGE ABUSE</b><span>Authority is excessive, confused, or stolen.</span></div>
  <div class="risk-card"><b style="color:#788b94">ASI04 · AGENTIC SUPPLY CHAIN</b><span>Models, tools, or dependencies are compromised.</span></div>
  <div class="risk-card focus"><b>ASI05 · UNEXPECTED CODE EXECUTION</b><span>Generated or triggered code runs unexpectedly.</span></div>
  <div class="risk-card"><b style="color:#788b94">ASI06 · MEMORY & CONTEXT POISONING</b><span>Durable context changes future behaviour.</span></div>
  <div class="risk-card"><b style="color:#788b94">ASI07 · INSECURE INTER-AGENT COMMUNICATION</b><span>Messages cross trust boundaries.</span></div>
  <div class="risk-card focus"><b>ASI08 · CASCADING FAILURES</b><span>One bad decision propagates across systems.</span></div>
  <div class="risk-card"><b style="color:#788b94">ASI09 · HUMAN-AGENT TRUST EXPLOITATION</b><span>People over-trust an agent’s output or action.</span></div>
  <div class="risk-card focus"><b>ASI10 · ROGUE AGENTS</b><span>The agent acts outside its intended objectives.</span></div>
</div>

<div class="source-note"><span class="focus-key">HIGHLIGHTED</span> = especially relevant to this talk · OWASP gives us vocabulary, not a risk-free system.</div>

<!--
If you have experience with traditional web application security, you might know the OWASP Top 10, which lists the most common risks for web applications and microservices.

Similarly, the OWASP Agentic Top 10 lists the most common risks for AI agents.
Here is the complete OWASP Agentic Top 10. We do not need to memorise ten risk categories today, but a shared risk taxonomy is useful.

The concerns here are a bit different from traditional web application security. An agent can interpret goals, choose tools, retain memory, delegate to other agents, and take actions. So the questions expand from “can this request reach the service?” to “what is the agent trying to do, what authority does it have, and what can happen next?”

The highlighted items are closer to governance controls in this talk: goal hijack, tool misuse, identity and privilege abuse, unexpected code execution, cascading failures, and rogue agents.

The other categories matter too. They remind us that governance is not only a pre-tool-call filter: it also includes supply chain, memory, inter-agent communication, and how people trust the system.

OWASP helps us ask the right questions. Governance controls and engineering practices are how we start answering them.
-->

---

<div class="eyebrow">07 / THE CONTROL POINT</div>

# The control point is the action boundary

<div class="action-flow">
  <div class="flow-box"><small>01</small><strong>Agent requests</strong><span>tool + arguments</span></div>
  <div class="flow-arrow">→</div>
  <div class="flow-box decision"><small>02</small><strong>Policy decides</strong><span>context + rules</span></div>
  <div class="flow-arrow">→</div>
  <div class="flow-box"><small>03</small><strong>Host applies</strong><span>execute / stop</span></div>
</div>

<div class="outcome-row"><span class="allow">EXECUTE</span><span class="warn">APPROVE</span><span class="deny">BLOCK</span><span class="transform">TRANSFORM</span><span class="evidence">RECORD</span></div>

<p class="lede compact">The policy layer decides. The host enforces.</p>

<!--
So how do we enforce governance for AI agents?
The most important control point is the action boundary: the moment when the agent is about to call a tool.

When an agent is trying to call a tool, we need to have a policy layer here to evaluate the request and decide whether it should be allowed, denied, escalated, or transformed. The host then applies the decision.

If the decision is allow, the tool can run. If it is deny, the tool must not run. If it is escalate, the host can ask for human approval. If it is transform, the host can change the action before execution.

This separation is important. The component that decides whether an action is allowed does not have to be the component that executes the action.

It creates a clear enforcement boundary that can work across different models and agent frameworks.
-->

---

<div class="eyebrow">08 / THE TOOLKIT</div>

# Where Agent Governance Toolkit fits

<div class="toolkit-layout">
  <div class="toolkit-visual">
    <img src="/toolkit-home.png" alt="Screenshot of the Agent Governance Toolkit documentation website" />
    <div class="toolkit-caption">Agent Governance Toolkit documentation · public preview · microsoft.github.io/agent-governance-toolkit</div>
  </div>
  <div class="toolkit-copy">
    <p>One engineering implementation of runtime governance.</p>
    <div class="toolkit-stack">
      <div class="stack-item active">POLICY ENFORCEMENT</div>
      <div class="stack-item">IDENTITY & TRUST</div>
      <div class="stack-item">RUNTIME ISOLATION</div>
      <div class="stack-item">AUDIT EVIDENCE</div>
      <div class="stack-item">RELIABILITY CONTROLS</div>
    </div>
    <ul>
      <li>Not a replacement for organisational policy</li>
      <li>Not a compliance certification</li>
      <li>Currently a public preview</li>
    </ul>
  </div>
</div>

<!--
This is where the Agent Governance Toolkit fits. It's an open-source project that provides engineering building blocks for runtime governance, including policy enforcement, identity and trust, runtime isolation, audit evidence, and reliability controls.

In this talk, I am focusing on one part: enforcing policy at the tool-call boundary.

The toolkit is not a replacement for an organisation’s governance process. It cannot decide your risk appetite, define your business policy, or assign accountability. It's just a practical implementation of AI runtime governance.

It's just an option, not the only way to implement governance.
-->

---

<div class="eyebrow">09 / THE CONTRACT</div>

# What is ACS?

<div class="acs-title">Agent Control Specification</div>
<div class="acs-subtitle">A contract between the agent host and the policy layer</div>

<div class="acs-flow"><span>ACTION CONTEXT</span><b>→</b><span class="acs-core">ACS POLICY DECISION</span><b>→</b><span>HOST ENFORCEMENT</span></div>

<div class="verdict-row"><span>allow</span><span>warn</span><span>deny</span><span>escalate</span><span>transform</span></div>

<div class="acs-footnote">Agent Control Specification ≠ Azure Communication Services</div>

<!--
To make this toolkit better fit into multiple scenarios, it introduces the concept of the Agent Control Specification (ACS).
Just like when you develop an API, first you define the contract for your client and server, ACS defines the contract between the agent host and the policy layer.

In this context, ACS is a contract between an agent host and a governance policy layer. The host provides the context for an action: which agent is acting, which session it belongs to, which tool is being called, and with which arguments.

The policy layer returns a normalised decision, such as allow, warn, deny, escalate, or transform.

ACS does not execute the action itself. It only provides a policy decision to the host. The host receives the decision and is responsible for enforcing it.
-->

---

<div class="eyebrow">10 / THE DEMO</div>

# A governed support agent

<div class="demo-architecture">
  <div class="provider primary-provider">AZURE AI FOUNDRY<br><small>model provider</small></div>
  <div class="demo-arrow">→</div>
  <div class="demo-agent">AGENT<br><small>one action at a time</small></div>
  <div class="demo-arrow">→</div>
  <div class="governed-boundary">GOVERNED<br>TOOL BOUNDARY</div>
</div>

<div class="demo-tools"><span class="allow">lookup_customer</span><span class="warn">send_email</span><span class="deny">delete_record</span></div>

<div class="demo-note">Azure AI Foundry · mock customer records · real email via Azure Communication Services</div>

<!--
Now let’s look at a small demo.

This is a fictional customer-support agent using deterministic mock customer records. The model runs through Azure AI Foundry, and the email tool uses Azure Communication Services for a real email side effect.

The important part is that the model provider does not define the governance boundary. The Foundry model produces tool calls that go through the same governed tool interface.

We have three tools with different risk levels: looking up a customer, sending an email, and deleting a record.
-->

---

<div class="eyebrow">11 / LIVE DECISIONS</div>

# Allow. Require approval. Deny.

<div class="decision-list">
  <div class="decision-line allow-line"><div class="decision-label allow">ALLOW</div><code>lookup_customer</code><span>tool executes</span></div>
  <div class="decision-line warn-line"><div class="decision-label warn">REQUIRE APPROVAL</div><code>send_email</code><span>approval, then execute</span></div>
  <div class="decision-line deny-line"><div class="decision-label deny">DENY</div><code>delete_record</code><span>tool is never called</span></div>
</div>

<div class="code-callout"><code>safe_tool = govern(my_tool, policy="policy.yaml")</code></div>

<!--
Let’s start with a harmless read operation.

The agent requests lookup_customer. The policy evaluates the action and returns allow. The host applies that decision, and the local tool executes. The audit record contains the agent, session, tool name, decision, and result.

Now let’s try sending an email. This is different. The policy does not necessarily reject it, but it does not allow the agent to do it autonomously either. In the Agent Governance Toolkit policy file, the native action is `require_approval` — not `escalate`.

The toolkit invokes the configured approval handler. In this small demo, that handler represents the meetup operator, and I can approve or reject the request. Only after approval does the email tool run.

After approval, this tool calls Azure Communication Services and sends a real test email to the configured controlled mailbox. The governance decision happens before the external side effect.

Finally, let’s try to delete a customer record. The policy returns deny. The important detail is not just the error message. The underlying tool function is never called.

This is the enforcement boundary: the model can request an action, but the agent host controls whether that action reaches the real tool.
-->

---

<div class="eyebrow">12 / THE TAKEAWAY</div>

# Accountability is an engineering requirement

<div class="checklist">
  <div><b>01</b><span>What tools can the agent call?</span></div>
  <div><b>02</b><span>What arguments are allowed?</span></div>
  <div><b>03</b><span>Which actions require approval?</span></div>
  <div><b>04</b><span>Can we identify the agent and session?</span></div>
  <div><b>05</b><span>Can we reconstruct what happened?</span></div>
</div>

<div class="final-quote">Autonomy without accountability is just uncontrolled access.</div>

<div class="question-prompt">Questions?</div>

<!--
The main lesson is not that every agent must use this particular toolkit.

The lesson is that once an agent can act, governance becomes part of the engineering design.

Before shipping an agent, ask five questions. What tools can it call? What arguments are allowed? Which actions require approval? Can we identify the agent, the user, and the session? And can we reconstruct what happened after an incident?

Agent Governance Toolkit is one way to implement some of these controls at runtime. Other architectures and tools are possible.

But the responsibility remains the same: build agents that are not only autonomous, but also observable, constrained, secure, and accountable.

Because autonomy without accountability is just uncontrolled access.
-->

<!-- takeaway is the final slide -->

<!--
Thank you. I’m happy to discuss the governance model, the toolkit, or the demo implementation.
-->

<style>
:root {
  --ink: #e8f3f7;
  --muted: #9ab2bd;
  --line: #294350;
  --panel: #10232e;
  --cyan: #63d6e8;
  --amber: #f3bd67;
  --rose: #ff7d8b;
  --green: #7de0a3;
}
.slidev-layout { background: #07131c; color: var(--ink); font-family: Inter, ui-sans-serif, system-ui, sans-serif; }
.slidev-layout h1 { color: var(--ink); font-size: 3.35rem; line-height: 1.05; letter-spacing: -0.045em; font-weight: 800; }
.slidev-layout h2 { color: var(--cyan); font-size: 1.15rem; letter-spacing: 0.03em; font-weight: 500; }
.slidev-layout p, .slidev-layout li { color: var(--muted); }
.slidev-layout code, .slidev-layout .tool-code, .slidev-layout .decision-label, .slidev-layout .eyebrow, .slidev-layout .panel-label, .slidev-layout .acs-footnote, .slidev-layout .source-note { font-family: 'JetBrains Mono', ui-monospace, monospace; }
.slidev-layout .eyebrow { color: var(--cyan); font-size: 0.7rem; letter-spacing: 0.18em; margin-bottom: 1.25rem; }
.slidev-layout .lede { color: var(--muted); font-size: 1.18rem; line-height: 1.45; max-width: 720px; }
.slidev-layout .lede.compact { margin-top: 1.8rem; font-size: 1rem; }
.slidev-layout .hero-mark { color: var(--cyan); font: 0.7rem 'JetBrains Mono', monospace; letter-spacing: 0.2em; margin-bottom: 4.5rem; }
.slidev-layout .speaker-line { color: #d8e7eb; font-size: 0.85rem; margin-top: 3rem; }
.slidev-layout .mvp-badge { position: absolute; right: 24px; bottom: 16px; display: block; width: 165px; max-width: none; height: 62px; }
.slidev-layout .hero-rule { height: 2px; width: 190px; background: linear-gradient(90deg, var(--cyan), transparent); margin: 1.35rem 0; }
.slidev-layout .event-line { color: var(--muted); font: 0.7rem 'JetBrains Mono', monospace; margin-top: 0.5rem; }
.slidev-layout .question-strip, .slidev-layout .bottom-line { border-left: 3px solid var(--cyan); background: rgba(99, 214, 232, 0.08); padding: 0.85rem 1rem; margin-top: 2rem; color: #d7edf1; }
.slidev-layout .tool-grid { display: grid; gap: 0.9rem; margin-top: 2.2rem; }
.slidev-layout .tool-grid.four { grid-template-columns: repeat(4, 1fr); }
.slidev-layout .tool-card, .slidev-layout .big-question, .slidev-layout .risk-card { background: var(--panel); border: 1px solid var(--line); border-radius: 0.35rem; padding: 1rem; }
.slidev-layout .tool-card { min-height: 8rem; display: flex; flex-direction: column; justify-content: space-between; }
.slidev-layout .tool-card strong { font-size: 0.92rem; color: var(--ink); font-family: 'JetBrains Mono', monospace; }
.slidev-layout .tool-card small, .slidev-layout .big-question span, .slidev-layout .risk-card span { color: var(--muted); font-size: 0.72rem; line-height: 1.35; }
.slidev-layout .tool-code { font-size: 0.6rem; letter-spacing: 0.15em; }
.slidev-layout .safe { border-top: 3px solid var(--green); }.slidev-layout .safe .tool-code, .slidev-layout .allow { color: var(--green); }
.slidev-layout .caution { border-top: 3px solid var(--amber); }.slidev-layout .caution .tool-code, .slidev-layout .warn { color: var(--amber); }
.slidev-layout .danger { border-top: 3px solid var(--rose); }.slidev-layout .danger .tool-code, .slidev-layout .deny { color: var(--rose); }
.slidev-layout .question-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; margin-top: 2.6rem; }
.slidev-layout .big-question { min-height: 10rem; display: flex; flex-direction: column; gap: 0.8rem; }
.slidev-layout .big-question strong { color: var(--ink); font-size: 1.05rem; line-height: 1.2; }
.slidev-layout .question-number { color: var(--cyan); font: 2rem 'JetBrains Mono', monospace; }
.slidev-layout .system-map { margin: 2.1rem auto 1.5rem; max-width: 760px; text-align: center; }
.slidev-layout .map-row { display: flex; justify-content: center; align-items: center; gap: 0.8rem; }
.slidev-layout .map-node { border: 1px solid var(--line); background: var(--panel); padding: 0.8rem 1.1rem; font: 0.68rem 'JetBrains Mono', monospace; letter-spacing: 0.08em; }
.slidev-layout .map-node.primary { border-color: var(--cyan); color: var(--cyan); box-shadow: 0 0 24px rgba(99, 214, 232, 0.14); }
.slidev-layout .map-arrow { color: var(--cyan); font-size: 1.3rem; }
.slidev-layout .map-branch { display: flex; justify-content: center; gap: 0.8rem; margin: 1.1rem 0; padding-top: 1.2rem; border-top: 1px dashed var(--line); }
.slidev-layout .map-effect { color: var(--rose); font: 0.72rem 'JetBrains Mono', monospace; letter-spacing: 0.18em; margin-top: 1rem; }
.slidev-layout .control-row { display: flex; justify-content: center; gap: 0.55rem; flex-wrap: wrap; }
.slidev-layout .control-row span, .slidev-layout .micro-grid span { border: 1px solid var(--line); color: var(--muted); padding: 0.45rem 0.7rem; font: 0.62rem 'JetBrains Mono', monospace; letter-spacing: 0.08em; }
.slidev-layout .two-panel { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-top: 2.1rem; }
.slidev-layout .panel { background: var(--panel); border: 1px solid var(--line); padding: 1.2rem 1.4rem; min-height: 10rem; }
.slidev-layout .panel-label { color: var(--cyan); font-size: 0.65rem; letter-spacing: 0.15em; }
.slidev-layout .panel ul { margin-top: 1rem; line-height: 1.8; }
.slidev-layout .verdicts { display: flex; flex-wrap: wrap; gap: 0.55rem; margin-top: 1.8rem; }
.slidev-layout .verdicts span, .slidev-layout .verdict-row span { border: 1px solid var(--amber); color: var(--amber); padding: 0.55rem 0.7rem; font: 0.67rem 'JetBrains Mono', monospace; }
.slidev-layout .micro-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 0.55rem; margin-top: 1.5rem; }
.slidev-layout .micro-grid span { text-align: center; font-size: 0.55rem; }
.slidev-layout .risk-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.8rem; margin-top: 1.8rem; }
.slidev-layout .risk-card { min-height: 0; display: flex; flex-direction: column; gap: 0.28rem; padding: 0.62rem 0.72rem; }
.slidev-layout .risk-card b { color: #788b94; font: 0.56rem 'JetBrains Mono', monospace; letter-spacing: 0.045em; line-height: 1.15; }
.slidev-layout .risk-card span { font-size: 0.54rem; line-height: 1.2; }
.slidev-layout .risk-card.focus { border-color: var(--cyan); background: rgba(99, 214, 232, 0.07); }.slidev-layout .risk-card.focus b { color: var(--cyan); }
.slidev-layout .focus-key { color: var(--cyan); }
.slidev-layout .source-note { color: var(--muted); font-size: 0.62rem; margin-top: 1rem; }
.slidev-layout .action-flow { display: flex; align-items: stretch; justify-content: center; gap: 0.65rem; margin-top: 2.6rem; }
.slidev-layout .flow-box { background: var(--panel); border: 1px solid var(--line); width: 190px; min-height: 7.6rem; padding: 1rem; display: flex; flex-direction: column; justify-content: space-between; }
.slidev-layout .flow-box.decision { border-color: var(--cyan); }.slidev-layout .flow-box small { color: var(--cyan); font: 0.65rem 'JetBrains Mono', monospace; }.slidev-layout .flow-box strong { color: var(--ink); }.slidev-layout .flow-box span { color: var(--muted); font-size: 0.7rem; }.slidev-layout .flow-arrow { color: var(--cyan); align-self: center; font-size: 1.5rem; }
.slidev-layout .outcome-row { display: flex; justify-content: center; gap: 0.6rem; margin-top: 1.4rem; flex-wrap: wrap; }.slidev-layout .outcome-row span { padding: 0.5rem 0.7rem; border: 1px solid currentColor; font: 0.62rem 'JetBrains Mono', monospace; }.slidev-layout .outcome-row .transform { color: var(--cyan); }.slidev-layout .outcome-row .evidence { color: var(--muted); }
.slidev-layout .toolkit-layout { display: grid; grid-template-columns: 1.18fr 0.82fr; gap: 1.2rem; align-items: start; margin-top: 1.25rem; }.slidev-layout .toolkit-visual img { display: block; width: 100%; max-height: 13.2rem; object-fit: cover; object-position: top; border: 1px solid var(--line); border-radius: 0.25rem; }.slidev-layout .toolkit-caption { color: var(--muted); font-size: 0.52rem; line-height: 1.25; margin-top: 0.42rem; }.slidev-layout .toolkit-stack { display: grid; gap: 0.38rem; margin-top: 0.85rem; }.slidev-layout .stack-item { border: 1px solid var(--line); color: var(--muted); padding: 0.46rem 0.58rem; font: 0.57rem 'JetBrains Mono', monospace; }.slidev-layout .stack-item.active { border-color: var(--cyan); color: var(--cyan); background: rgba(99, 214, 232, 0.08); }.slidev-layout .toolkit-copy p { color: var(--ink); font-size: 1rem; line-height: 1.2; margin: 0; }.slidev-layout .toolkit-copy li { margin: 0.34rem 0; font-size: 0.66rem; line-height: 1.2; }
.slidev-layout .acs-title { color: var(--ink); font-size: 2rem; font-weight: 700; margin-top: 2.5rem; }.slidev-layout .acs-subtitle { color: var(--muted); margin-top: 0.45rem; }.slidev-layout .acs-flow { display: flex; align-items: center; justify-content: center; gap: 1rem; margin-top: 2.3rem; }.slidev-layout .acs-flow span { border: 1px solid var(--line); padding: 0.85rem 1rem; font: 0.62rem 'JetBrains Mono', monospace; }.slidev-layout .acs-flow b { color: var(--cyan); }.slidev-layout .acs-flow .acs-core { border-color: var(--cyan); color: var(--cyan); background: rgba(99, 214, 232, 0.08); }.slidev-layout .verdict-row { display: flex; justify-content: center; gap: 0.55rem; margin-top: 1.5rem; }.slidev-layout .verdict-row span { border-color: var(--cyan); color: var(--cyan); }.slidev-layout .acs-footnote { color: var(--muted); text-align: center; font-size: 0.62rem; margin-top: 2.2rem; }
.slidev-layout .demo-architecture { display: flex; align-items: center; justify-content: center; gap: 0.8rem; margin-top: 2rem; }.slidev-layout .provider-column { display: grid; gap: 0.5rem; }.slidev-layout .provider, .slidev-layout .demo-agent, .slidev-layout .governed-boundary { background: var(--panel); border: 1px solid var(--line); padding: 0.75rem 0.9rem; text-align: center; font: 0.65rem 'JetBrains Mono', monospace; }.slidev-layout .provider small, .slidev-layout .demo-agent small { color: var(--muted); font: 0.55rem Inter, sans-serif; }.slidev-layout .primary-provider { border-color: var(--cyan); color: var(--cyan); }.slidev-layout .demo-agent { border-color: var(--amber); color: var(--amber); min-width: 140px; }.slidev-layout .governed-boundary { border-color: var(--green); color: var(--green); min-width: 160px; }.slidev-layout .demo-arrow { color: var(--cyan); font-size: 1.4rem; }.slidev-layout .demo-tools { display: flex; justify-content: center; gap: 0.7rem; margin-top: 1.7rem; }.slidev-layout .demo-tools span { border: 1px solid currentColor; padding: 0.65rem 0.8rem; font: 0.64rem 'JetBrains Mono', monospace; }.slidev-layout .demo-note { text-align: center; color: var(--muted); font: 0.62rem 'JetBrains Mono', monospace; margin-top: 1.4rem; }
.slidev-layout .ungoverned-boundary { background: var(--panel); border: 1px solid var(--rose); color: var(--rose); min-width: 160px; padding: 0.75rem 0.9rem; text-align: center; font: 0.65rem 'JetBrains Mono', monospace; }
.slidev-layout .decision-list { display: grid; gap: 0.8rem; margin-top: 2rem; }.slidev-layout .decision-line { display: grid; grid-template-columns: 125px 1fr 1fr; align-items: center; gap: 1rem; border-left: 3px solid currentColor; background: var(--panel); padding: 0.85rem 1rem; }.slidev-layout .decision-label { font-size: 0.66rem; letter-spacing: 0.12em; }.slidev-layout .decision-line code { color: var(--ink); }.slidev-layout .decision-line span:last-child { color: var(--muted); font-size: 0.72rem; }.slidev-layout .allow-line { color: var(--green); }.slidev-layout .warn-line { color: var(--amber); }.slidev-layout .deny-line { color: var(--rose); }.slidev-layout .code-callout { margin-top: 1.6rem; background: #050d13; border: 1px solid var(--line); padding: 0.8rem 1rem; color: var(--cyan); text-align: center; }
.slidev-layout .checklist { display: grid; grid-template-columns: 1fr 1fr; gap: 0.65rem 1rem; margin-top: 1.7rem; }.slidev-layout .checklist div { display: flex; gap: 0.9rem; align-items: baseline; border-bottom: 1px solid var(--line); padding: 0.7rem 0; }.slidev-layout .checklist b { color: var(--cyan); font: 0.68rem 'JetBrains Mono', monospace; }.slidev-layout .checklist span { color: var(--ink); font-size: 0.88rem; }.slidev-layout .final-quote { color: var(--amber); font-size: 1.45rem; font-weight: 700; margin-top: 2.4rem; letter-spacing: -0.02em; }.slidev-layout .question-prompt { color: var(--cyan); font: 0.78rem 'JetBrains Mono', monospace; letter-spacing: 0.15em; margin-top: 1.8rem; }
</style>
