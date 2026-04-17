# Global Azure Bootcamp 2026 – Microsoft Agent Framework Demos

A .NET 10 solution showcasing [Microsoft Agent Framework (MAF)](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview) features, built for the Global Azure Bootcamp 2026 talk.

## Solution Structure

```
GlobalAzureBootcamp2026Demo.sln
├── src/
│   └── SharedConfig/              # Shared class library — Azure OpenAI config via user secrets
└── demos/
    ├── Demo01_AgentWithTools/     # Demo 1: Agent with function tools
    ├── Demo02_AgentWithSkills/    # Demo 2: Agent with skills (file-based + inline)
    │   └── skills/
    │       └── conference-guide/ # SKILL.md, references/, assets/, scripts/
    └── Demo03_ToolsVsSkills/      # Demo 3: Tools vs Skills — when to use each
        └── skills/
            ├── product-docs/     # Getting-started & error code reference
            ├── billing-policy/   # Pricing tiers & refund policy (live-edited on stage)
            └── troubleshooting/  # 503, 429, and auth error guides
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Python 3 (`python3`) — required by Demo 02 to run the schedule script
- An Azure OpenAI resource deployed in [Azure AI Foundry](https://ai.azure.com/) with a chat model (e.g. `gpt-4o`)
- `az login` completed (used by `DefaultAzureCredential`)

---

## One-Time Setup: Configure User Secrets

All demos share the **same** `UserSecretsId` (`6e990080-ffbf-4bed-8b2d-abb21b747384`), so you only ever run these two commands once — from any demo project directory:

```bash
cd demos/Demo01_AgentWithTools

dotnet user-secrets set "AzureOpenAI:Endpoint"       "https://<your-resource>.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
```

Every project in the solution already has `<UserSecretsId>6e990080-ffbf-4bed-8b2d-abb21b747384</UserSecretsId>` in its `.csproj`, pointing to the same local secrets store. No per-project setup needed.

---

## Demos

### Demo 01 – Agent with Tools (`demos/Demo01_AgentWithTools`)

**What it shows:** How to register C# methods as tools (using `[Description]` attributes) so that a MAF `AIAgent` can call your code when relevant — instead of hallucinating answers.

**Use case:** A conference assistant for Global Azure Bootcamp 2026 that can answer attendee questions by calling real functions:

| Tool | Purpose |
|------|---------|
| `GetSessionsByTrack(track)` | Lists sessions for AI / Cloud / DevOps / Security |
| `GetSessionDetails(title)` | Returns room, time, and abstract for a session |
| `GetSpeakerBio(name)` | Returns a speaker's bio and company |
| `GetCurrentConferenceTime()` | Returns the current simulated conference time |

**Key MAF concepts demonstrated:**
- `AIFunctionFactory.Create()` — wraps a method as an `AITool`
- `ChatClient.AsAIAgent(tools: [...])` — creates the agent with tools registered
- `agent.RunAsync()` — non-streaming invocation
- `agent.RunStreamingAsync()` — streaming invocation
- Interactive REPL loop for live demos

**Run it:**

```bash
cd demos/Demo01_AgentWithTools
dotnet run
```

The program runs three scripted demo queries then opens an interactive prompt.

---

### Demo 02 – Agent with Skills (`demos/Demo02_AgentWithSkills`)

**What it shows:** How to give an agent portable, self-contained domain knowledge via **Agent Skills** — packages of instructions, reference documents, assets, and executable scripts that the agent loads on demand.

**Use case:** A conference concierge for Global Azure Bootcamp 2026 that answers venue, logistics, schedule, sponsor, and announcement questions — all from skills, not hardcoded knowledge.

**Skill package layout:**

```
skills/conference-guide/
├── SKILL.md                  # Skill instructions (loaded by agent on demand)
├── references/
│   └── venue-faq.md          # Detailed FAQ — read on demand
├── assets/
│   └── sponsor-info.md       # Sponsor booth details — read on demand
└── scripts/
    └── get_schedule.py       # Python schedule generator — run on demand
```

**Demo queries and what each one demonstrates:**

| Demo | Query | Concept |
|------|-------|---------|
| A | Venue & transport directions | Skill load (`load_skill`) |
| B | Lunch & dietary options | Read reference (`read_skill_resource`) |
| C | Full AI track schedule | Run script (`run_skill_script` → Python) |
| D | Announcements & last-minute changes | Inline code-defined skill |
| E | Sponsor booths | Read asset (`read_skill_resource`) |
| F | Follow-up on sponsors | Multi-turn memory (`AgentSession`) |

**Key MAF concepts demonstrated:**
- `AgentSkillsProviderBuilder` — assembles file-based and inline skill sources
- `AgentFileSkillsSource` — auto-discovers skill packages from a directory
- Progressive disclosure — agent advertises skills, loads full content only when needed
- `read_skill_resource` — fetches reference docs and assets from within a skill
- `run_skill_script` + `SubprocessScriptRunner` — executes a Python script bundled in the skill
- `AgentInlineSkill` — code-defined skill with a dynamic resource (live announcements)
- `AgentSession` — multi-turn conversation memory

**Run it:**

```bash
cd demos/Demo02_AgentWithSkills
dotnet run
```

The program runs six scripted demo queries (A–F) then opens an interactive prompt.

> **Note:** Demo C runs `scripts/get_schedule.py` via Python 3. Ensure `python3` is on your PATH.

---

### Demo 03 – Tools vs Skills (`demos/Demo03_ToolsVsSkills`)

**What it shows:** The *real* difference between tools and skills — demonstrated side-by-side in one agent serving a fictitious CloudStack SaaS support desk. The agent decides which pattern to use per query automatically.

| Pattern | Right choice when… | Example |
|---------|---------------------|---------|
| **Tool** | Answer changes in real time, needs a side effect, or is per-user | Service health, account lookup, open ticket |
| **Skill** | Answer is stable knowledge that a non-developer could maintain | Troubleshooting guide, refund policy |

**Tools registered:**

| Tool | Why it must be a tool |
|------|----------------------|
| `GetServiceHealth(service)` | Live status — changes by the minute |
| `GetCustomerAccount(email)` | Per-user data — can't be in a static file |
| `CreateSupportTicket(title, description, priority)` | Side effect — writes to a system |

**Skills loaded from `skills/`:**

| Skill | Resources | Why it must be a skill |
|-------|-----------|----------------------|
| `product-docs` | `references/getting-started.md`, `references/error-codes.md` | Developer docs — stable, no live data needed |
| `billing-policy` | (inline in SKILL.md) | Business policy — owned by non-devs, editable without recompile |
| `troubleshooting` | `references/503-guide.md`, `references/429-guide.md`, `references/auth-errors.md` | Support runbooks — change rarely, non-dev owned |

**Demo queries and what each one demonstrates:**

| Demo | Query | Pattern | Why |
|------|-------|---------|-----|
| A | "Is the CloudStack API healthy?" | Tool | Live data — status changes minute to minute |
| B | "How do I fix a 503 error?" | Skill | Stable runbook — same steps every time |
| C | "What is the refund policy?" | Skill | Business policy — see Demo F |
| D | "Open a support ticket for the API latency" | Tool | Side effect — creates a record |
| E | "Check my account + explain Pro plan + open upgrade ticket" | **Both** | Needs live account data AND billing policy |
| F | *(live edit of SKILL.md, no recompile, re-run C)* | Skill | Shows non-dev ownership — policy updates instantly |

**The "aha moment" — Demo F:**

The program pauses and prompts you to edit `skills/billing-policy/SKILL.md` on stage:

```
Change: 'Refund window: 7 days' → 'Refund window: 30 days'
```

Press Enter. The same refund policy question (Demo C) runs again — the agent now says **30 days**.
No recompile. No redeploy. A non-developer changed the policy.

**Key MAF concepts demonstrated:**
- `AIFunctionFactory.Create()` — tools for live data and side effects
- `AgentSkillsProviderBuilder` — skills for stable domain knowledge
- `ChatOptions.Tools` — how tools are wired into the agent
- `ChatClientAgentOptions.AIContextProviders` — how skills are wired in
- Both registered to the same agent — it picks the right pattern automatically
- Live skill editing — no recompile needed to update knowledge

**Run it:**

```bash
cd demos/Demo03_ToolsVsSkills
dotnet run
```

The program runs scripted demos A–E, pauses for the live edit, runs Demo F, then opens an interactive prompt.

---

## How SharedConfig Works

`SharedConfig` is a .NET 10 class library referenced by every demo project. It exposes a single factory method:

```csharp
var (client, deploymentName) = AgentClientFactory.Create();
```

This reads `AzureOpenAI:Endpoint` and `AzureOpenAI:DeploymentName` from user secrets and returns a configured `AzureOpenAIClient` authenticated via `DefaultAzureCredential`. No API keys in code or config files.

---

## Adding More Demos

1. Create a new console project under `demos/`:
   ```bash
   dotnet new console -n Demo04_MultiAgent -o demos/Demo04_MultiAgent -f net10.0
   ```
2. Add a reference to SharedConfig:
   ```bash
   cd demos/Demo04_MultiAgent
   dotnet add reference ../../src/SharedConfig/SharedConfig.csproj
   ```
3. Set the **shared** UserSecretsId in the new `.csproj` (same value, no new `dotnet user-secrets set` needed):
   ```xml
   <UserSecretsId>6e990080-ffbf-4bed-8b2d-abb21b747384</UserSecretsId>
   ```
   Or run `dotnet user-secrets init` then replace the generated ID with the shared one above.
4. Add the project to the solution:
   ```bash
   cd ../..
   dotnet sln add demos/Demo04_MultiAgent/Demo04_MultiAgent.csproj
   ```
5. Call `AgentClientFactory.Create()` — no arguments needed, secrets are already there.
