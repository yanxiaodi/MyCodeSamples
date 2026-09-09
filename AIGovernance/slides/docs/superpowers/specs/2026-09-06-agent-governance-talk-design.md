# Agent Governance Meetup Talk Design

## Goal

Create a 20-minute English-language Slidev talk titled “Governing AI Agents: From Autonomy to Accountability”.

## Audience and thesis

The audience understands agents and LLM applications but has limited AI governance experience. The talk argues that an agent is a production system, not only code: once it can act, it needs policy, guardrails, identity, observability, security, audit evidence, and reliability controls.

## Content boundaries

- Explain governance as broader than runtime guardrails.
- Use OWASP Agentic Top 10 as a risk vocabulary, not as a full compliance lecture.
- Present Agent Governance Toolkit as one public-preview implementation option, not a complete governance programme.
- Explain Agent Control Specification (ACS) as the policy decision contract, explicitly distinguishing it from Azure Communication Services.
- Focus the toolkit demo on the tool-call enforcement boundary.

## Demo design

Use a fictional banking customer-support agent with local fake data and three tools:

- `lookup_customer`: allow and execute.
- `send_email`: require approval, simulate approval, then execute a fake or optional real email tool.
- `delete_record`: deny and prove the underlying tool was not called.

Azure AI Foundry is the preferred model provider behind a small interface, with a deterministic local mock fallback. The demo must not require Foundry or external email to build or preview the slides.

## Slide design

Use 12 sparse, takeaway-led slides. Put the full English speaker script in Slidev presenter notes. Keep code and diagrams minimal, readable, and focused on the action boundary.
