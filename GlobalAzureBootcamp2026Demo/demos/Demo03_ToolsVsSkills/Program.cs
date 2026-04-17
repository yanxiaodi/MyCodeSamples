// =============================================================================
// Global Azure Bootcamp 2026 – Demo 03: Tools vs Skills
// =============================================================================
// This demo shows the real difference between MAF Tools and MAF Skills by
// running BOTH side-by-side in a single agent serving a CloudStack support desk.
//
// Core distinction shown:
//   Tools  = agent ACTS   → live data, side effects, per-user state
//   Skills = agent KNOWS  → domain knowledge authored outside the codebase
//
// Use case: CloudStack customer support agent that:
//   • Checks live service health              (tool — changes by the minute)
//   • Explains how to fix a 503 error         (skill — stable reference guide)
//   • Quotes the refund policy                (skill — policy, non-dev owned)
//   • Opens a support ticket                  (tool — writes to a system)
//   • Does all of the above in one message    (tool + skill together)
//
// The "aha moment" is Demo F: the presenter edits billing-policy/SKILL.md live
// on stage (changes "7 days" → "30 days") without recompiling — and the agent
// immediately answers with the updated policy.
//
// Key concepts shown:
//   • AIFunctionFactory.Create — registers C# methods as tools
//   • AgentSkillsProvider — loads SKILL.md packages from disk
//   • Agent selecting the right pattern per query automatically
//   • Live skill editing without recompilation
//   • AgentSession — multi-turn memory across all queries
// =============================================================================

using Demo03_ToolsVsSkills;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using SharedConfig;

var (openAIClient, deploymentName) = AgentClientFactory.Create();

// ---------------------------------------------------------------------------
// Resolve the skills directory to the PROJECT SOURCE folder when possible.
// When running in debug (bin/Debug/net10.0/), three levels up is the project
// root, so edits to skills/*.md are picked up immediately without a rebuild.
// Falls back to the copied output folder for release/published scenarios.
// ---------------------------------------------------------------------------
var outputSkillsDir = Path.Combine(AppContext.BaseDirectory, "skills");
var sourceSkillsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../skills"));
var skillsDir = Directory.Exists(sourceSkillsDir) &&
                Directory.EnumerateFiles(sourceSkillsDir, "SKILL.md", SearchOption.AllDirectories).Any()
    ? sourceSkillsDir
    : outputSkillsDir;

// ---------------------------------------------------------------------------
// Factory: builds a fresh AgentSkillsProvider + AIAgent + AgentSession.
// Called once at startup and again after the live-edit pause so that any
// in-memory skill cache inside AgentSkillsProvider is fully cleared.
// ---------------------------------------------------------------------------
async Task<(AIAgent agent, AgentSession session)> CreateAgentAsync()
{
    var provider = new AgentSkillsProviderBuilder()
        .UseFileSkill(skillsDir)
        .UseFileScriptRunner(SubprocessScriptRunner.RunAsync)
        .UseOptions(o =>
        {
            o.SkillsInstructionPrompt = """
                You have access to skills containing domain-specific knowledge.

                <available_skills>
                {skills}
                </available_skills>

                When a task aligns with a skill's domain, follow these steps in order:
                - Use `load_skill` to retrieve the skill's instructions.
                - Follow the provided guidance.
                {resource_instructions}
                {script_instructions}
                IMPORTANT: Resource names must be used exactly as shown in the skill instructions,
                including any directory prefix (e.g. use "references/503-guide.md" not just "503-guide.md").
                Only load what is needed, when it is needed.
                """;
        })
        .Build();

    AIAgent a = openAIClient
        .GetChatClient(deploymentName)
        .AsAIAgent(new ChatClientAgentOptions
        {
            Name = "CloudStack Support Agent",
            ChatOptions = new()
            {
                Instructions = """
                    You are a CloudStack customer support agent.
                    You help customers with service health, account questions, troubleshooting, billing, and opening tickets.

                    You have access to:
                    - TOOLS: Use for live data (service health), account lookups, and side effects (creating tickets).
                    - SKILLS: Use for knowledge — product documentation, troubleshooting guides, and billing policy.

                    Always use the most appropriate mechanism for each part of a request.
                    For a question that needs both live data AND policy knowledge, use both.
                    Be concise and actionable. Use bullet points where helpful.
                    """,
                Tools =
                [
                    AIFunctionFactory.Create(CloudStackTools.GetServiceHealth),
                    AIFunctionFactory.Create(CloudStackTools.GetCustomerAccount),
                    AIFunctionFactory.Create(CloudStackTools.CreateSupportTicket),
                ],
            },
            AIContextProviders = [provider],
        });

    return (a, await a.CreateSessionAsync());
}

var (agent, session) = await CreateAgentAsync();

PrintBanner(skillsDir);

// ---------------------------------------------------------------------------
// Scripted demo queries — each one makes the tools-vs-skills contrast visible.
// ---------------------------------------------------------------------------

// TOOL: Live data — a static file can never answer "is it down right now?"
await RunDemoQuery(
    label: "Demo A – TOOL: Live service health (calls GetServiceHealth)",
    query: "Is the CloudStack API currently healthy? Any active incidents?");

// SKILL: Stable knowledge — the fix steps don't change hour to hour
await RunDemoQuery(
    label: "Demo B – SKILL: Troubleshooting guide (loads troubleshooting skill, reads 503-guide.md)",
    query: "I'm getting HTTP 503 errors from the CloudStack API in my integration. How do I fix this?");

// SKILL: Business policy — editable without touching C#
await RunDemoQuery(
    label: "Demo C – SKILL: Refund policy (loads billing-policy skill)",
    query: "What is the CloudStack refund policy? How many days do I have to request a refund?");

// TOOL: Side effect — skills can't write to systems
await RunDemoQuery(
    label: "Demo D – TOOL: Create ticket (calls CreateSupportTicket)",
    query: "Please open a high-priority support ticket: 'API latency degradation affecting production jobs'. " +
           "The POST /v2/jobs endpoint is returning p95 latency of 4+ seconds.");

// BOTH: One message that needs live data AND policy knowledge
await RunDemoQuery(
    label: "Demo E – BOTH: Account lookup + plan advice (tool + skill together)",
    query: "My email is alice@contoso.com. Look up my account, explain what the Pro plan would give me " +
           "compared to my current plan, and open an upgrade enquiry ticket on my behalf.");

// Live-edit pause — the presenter edits billing-policy/SKILL.md on stage
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
var editTarget = Path.Combine(skillsDir, "billing-policy", "SKILL.md");
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ✏️   LIVE EDIT MOMENT                                        ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
Console.WriteLine($"║  File: {editTarget}");
Console.WriteLine("║                                                              ║");
Console.WriteLine("║  Change: 'Refund window: 7 days' → 'Refund window: 30 days' ║");
Console.WriteLine("║  Save the file, then press Enter to re-run.                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.Write("\n  [Waiting — edit the file, then press Enter] ");
Console.ReadLine();

// Rebuild the entire provider + agent + session after the edit.
// This guarantees no in-memory skill cache survives — regardless of MAF internals.
var (freshAgent, freshSession) = await CreateAgentAsync();

// Same question as Demo C — agent loads billing-policy skill fresh, reads updated SKILL.md
await RunDemoQuery(
    label: "Demo F – LIVE EDIT: Same question, updated skill, no recompile",
    query: "What is the CloudStack refund policy? How many days do I have to request a refund?",
    agentOverride: freshAgent,
    sessionOverride: freshSession);

// ---------------------------------------------------------------------------
// Interactive REPL
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("  Interactive mode — type a question or 'exit' to quit");
Console.WriteLine("═══════════════════════════════════════════════════════");

while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.WriteLine("\nAgent:");
    await foreach (var chunk in agent.RunStreamingAsync(input, session))
        Console.Write(chunk);

    Console.WriteLine();
}

Console.WriteLine("\nThanks for attending Global Azure Bootcamp 2026! 🎉");

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

async Task RunDemoQuery(string label, string query, AIAgent? agentOverride = null, AgentSession? sessionOverride = null)
{
    Console.WriteLine();
    Console.WriteLine($"┌─ {label}");
    Console.WriteLine($"│  Q: {query}");
    Console.WriteLine("│");
    Console.Write("│  A: ");

    var a = agentOverride ?? agent;
    await foreach (var chunk in a.RunStreamingAsync(query, sessionOverride ?? session))
        Console.Write(chunk);

    Console.WriteLine();
    Console.WriteLine("└─────────────────────────────────────────────────────");
}

void PrintBanner(string skillsDir)
{
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║   Global Azure Bootcamp 2026                                 ║");
    Console.WriteLine("║   Demo 03: Microsoft Agent Framework – Tools vs Skills       ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("  Agent: CloudStack Support");
    Console.WriteLine("  Tools:  GetServiceHealth · GetCustomerAccount · CreateSupportTicket");
    Console.WriteLine("  Skills: product-docs · billing-policy · troubleshooting");
    Console.WriteLine("  Skills dir: " + skillsDir);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  Key point: Tools = agent ACTS (live data, side effects)");
    Console.WriteLine("             Skills = agent KNOWS (stable knowledge, non-dev owned)");
    Console.ResetColor();
    Console.WriteLine();
}
