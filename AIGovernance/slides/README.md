# Governing AI Agents — Slidev talk

Slidev deck and deterministic Python demo for Xiaodi Yan's meetup talk:
**Governing AI Agents: From Autonomy to Accountability**.

To start the slide show:

## Slides

```powershell
npm install
npm run dev
```

Then open the local URL printed by Slidev.

Do not open `slides.md` as a normal Markdown file. If an old Slidev process
is holding the default port or the page appears unstyled, start a fresh server
on another port:

```powershell
npm run dev -- --port 3456
```

Build the static deck with:

```powershell
npm run build
```

## Demo

The Python demo is maintained separately at:

```text
D:\dev\MyCodeSamples\AIGovernance\demo
```

Open `D:\dev\MyCodeSamples\AIGovernance\demo` in VS Code and run:

```powershell
python governed_agent.py
```

Its `demo\.env` supports Azure AI Foundry and the optional Azure Communication Services connection string. The default path prefers Foundry when configured and falls back to the local mock if the model request fails. It demonstrates `allow`, `require_approval`, and `deny` at the tool boundary using the official Agent Governance Toolkit.

Edit [slides.md](./slides.md) to see the changes. Speaker notes are stored in the HTML comments for each slide and appear in Presenter Mode.

Learn more about Slidev at the [documentation](https://sli.dev/).
