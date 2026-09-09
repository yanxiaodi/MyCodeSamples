# Agent Governance Meetup Talk Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the starter Slidev deck with a polished 12-slide English talk and add a safe, configurable Python demo scaffold for the governed tool-call story.

**Architecture:** `slides.md` owns the deck, visual styles, and presenter notes. `demo/` owns the deterministic local governance simulation and optional provider configuration documentation. The deck does not import or execute the demo so Slidev remains buildable without Python, Azure credentials, or network access.

**Tech Stack:** Slidev 52, Vue/UnoCSS in Markdown, Mermaid, Python 3.11+ standard library.

**Spec:** `docs/superpowers/specs/2026-09-06-agent-governance-talk-design.md`

## Global Constraints

- Exactly 12 content slides in `slides.md`.
- Speaker notes are English and placed in Slidev HTML comments.
- Demo defaults to local deterministic behavior; external services are optional.
- Do not claim that toolkit mappings eliminate OWASP risks or replace organisational governance.
- Do not send real email during build or verification.

### Task 1: Replace the starter deck

**Files:**
- Modify: `slides.md`

**Interfaces:**
- Produces a Slidev deck with 12 slides, presenter notes, diagrams, code snippets, and a dark governance visual theme.

- [x] Replace starter frontmatter and sample slides with the 12-slide talk structure from the approved design.
- [x] Add presenter notes containing the agreed English script for every slide.
- [x] Add a compact visual system: dark navy canvas, cyan/amber/rose risk accents, readable code blocks, and consistent footer metadata.
- [x] Run `npm run build` and inspect the generated build output for a successful exit.

### Task 2: Add the deterministic demo scaffold

**Files:**
- Create: `demo/governed_agent.py`
- Create: `demo/README.md`
- Create: `demo/policy.yaml`
- Create: `.env.example`

**Interfaces:**
- `demo/governed_agent.py` exposes a CLI that runs the three demo actions locally and prints decisions plus audit events.
- Environment variables document optional Azure AI Foundry and Azure Communication Services configuration without requiring either service.

- [x] Implement a provider interface with deterministic mock behavior as the default.
- [x] Implement policy evaluation for allow, require approval, and deny without executing denied tools.
- [x] Implement fake customer tools and an in-memory audit trail.
- [x] Add optional provider/email notes without making network calls by default.
- [x] Run the CLI and verify the output contains one allow, one escalation followed by approval, one denial, and no execution marker for deletion.

### Task 3: Final verification and handoff

**Files:**
- Modify: `README.md`

- [x] Update README with the talk preview and demo commands.
- [x] Run `npm run build`.
- [x] Run `python demo/governed_agent.py`.
- [x] Re-read the plan and verify every requirement against the generated files.
