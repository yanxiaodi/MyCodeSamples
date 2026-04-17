// =============================================================================
// Global Azure Bootcamp 2026 – Demo 02: Agent with Skills
// =============================================================================
// This demo shows how to give a MAF AIAgent packaged domain expertise via
// Agent Skills — portable SKILL.md packages with bundled references and scripts.
//
// Use case: A conference concierge that knows venue logistics, FAQ, sponsors,
// and can generate the track schedule — all from a skill, not hardcoded in the agent.
//
// Key concepts shown:
//   • AgentSkillsProvider — points agent to a directory of skill packages
//   • Progressive disclosure — agent advertises skills, loads them on demand
//   • File-based resources — venue-faq.md and sponsor-info.md read on demand
//   • Script execution — get_schedule.py run via SubprocessScriptRunner
//   • Inline (code-defined) skill — dynamic content created in code
//   • AgentSession — multi-turn conversation memory
// =============================================================================

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using SharedConfig;

var (openAIClient, deploymentName) = AgentClientFactory.Create();

// ---------------------------------------------------------------------------
// 1. File-based skill: point to the 'skills' folder next to the binary.
//    Each subdirectory containing a SKILL.md is auto-discovered as a skill.
//    SubprocessScriptRunner enables the agent to execute bundled Python scripts.
// ---------------------------------------------------------------------------
var skillsDir = Path.Combine(AppContext.BaseDirectory, "skills");

// ---------------------------------------------------------------------------
// 2. Inline (code-defined) skill: dynamic content generated at runtime.
//    Great for agent-specific knowledge that lives alongside the app code.
// ---------------------------------------------------------------------------
var eventInfoSkill = new AgentInlineSkill(
    name: "event-announcements",
    description: "Latest announcements, last-minute changes, and real-time updates " +
                 "for Global Azure Bootcamp 2026. Use when attendees ask about changes, " +
                 "cancellations, prize draws, or today's news.",
    instructions: """
        You are the live announcement board for Global Azure Bootcamp 2026.
        When asked about updates or announcements, relay the items below accurately.
        """)
    .AddResource("todays-announcements", () =>
    {
        // In a real app this would be fetched from a CMS, database, or event API.
        var now = DateTime.Now;
        return $"""
            # Live Announcements — {now:dddd d MMMM yyyy}
            Last updated: {now:HH:mm}

            1. ⚠️  Room B sessions starting at 11:00 have moved to Room D due to AV setup.
            2. 🍕  Lunch service will start 15 minutes early today (11:45 AM) — queues expected!
            3. 🎉  Datacom prize draw (wireless earbuds) moved to 4:00 PM in the main atrium.
            4. 📢  Alex Rivera's keynote will be livestreamed — share the link: https://aka.ms/gab2026-live
            5. ☕  The Level 3 coffee station is temporarily out of oat milk. Level 2 still has stock.
            """;
    });

// ---------------------------------------------------------------------------
// 3. Combine both skill sources using AgentSkillsProviderBuilder.
//    Custom SkillsInstructionPrompt adds a note about script name directory
//    prefixes (the default prompt is vague; this avoids LLM dropping "scripts/").
// ---------------------------------------------------------------------------
var skillsProviderOptions = new AgentSkillsProviderOptions
{
    SkillsInstructionPrompt = """
        You have access to skills containing domain-specific knowledge and capabilities.
        Each skill provides specialized instructions, reference documents, and assets for specific tasks.

        <available_skills>
        {skills}
        </available_skills>

        When a task aligns with a skill's domain, follow these steps in exact order:
        - Use `load_skill` to retrieve the skill's instructions.
        - Follow the provided guidance.
        {resource_instructions}
        {script_instructions}
        IMPORTANT: Script and resource names must be used exactly as shown in the skill instructions,
        including any directory prefix (e.g. use "scripts/get_schedule.py" not just "get_schedule.py",
        and "references/venue-faq.md" not just "venue-faq.md").
        Only load what is needed, when it is needed.
        """,
};

var combinedSkillsProvider = new AgentSkillsProviderBuilder()
    .UseFileSkill(skillsDir)                                   // discovers conference-guide from skills/
    .UseSkill(eventInfoSkill)                                  // adds the inline dynamic-content skill
    .UseFileScriptRunner(SubprocessScriptRunner.RunAsync)      // enables run_skill_script
    .UseOptions(o =>
    {
        o.SkillsInstructionPrompt = skillsProviderOptions.SkillsInstructionPrompt;
    })                                                         // explicit script naming guidance
    .Build();

// ---------------------------------------------------------------------------
// 4. Create the agent with skills as AIContextProviders.
//    The agent's instructions are intentionally minimal — all domain knowledge
//    lives in the skills packages, demonstrating skills' portability.
// ---------------------------------------------------------------------------
AIAgent agent = openAIClient
    .GetChatClient(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "GAB2026 Concierge",
        ChatOptions = new()
        {
            Instructions = """
                You are the official concierge for Global Azure Bootcamp 2026.
                You help attendees with venue information, logistics, and the conference experience.
                Use your available skills to answer questions accurately.
                Keep answers friendly and concise. Use bullet points or tables where helpful.
                """,
        },
        AIContextProviders = [combinedSkillsProvider],
    });

// ---------------------------------------------------------------------------
// 5. Create a session for multi-turn memory.
// ---------------------------------------------------------------------------
AgentSession session = await agent.CreateSessionAsync();

PrintBanner();

// ---------------------------------------------------------------------------
// 6. Scripted demo queries — each demonstrates a different skill capability.
// ---------------------------------------------------------------------------

await RunDemoQuery(
    label: "Demo A – Skill load: Venue basics (triggers conference-guide skill)",
    query: "Where is the conference? How do I get there by public transport?");

await RunDemoQuery(
    label: "Demo B – Read reference: FAQ lookup (reads venue-faq.md resource)",
    query: "Is lunch included? What are the dietary options?");

await RunDemoQuery(
    label: "Demo C – Run script: Track schedule (runs get_schedule.py --track AI)",
    query: "Give me the full schedule for the AI track.");

await RunDemoQuery(
    label: "Demo D – Inline skill: Live announcements (reads dynamic resource)",
    query: "Are there any announcements or last-minute changes I should know about?");

await RunDemoQuery(
    label: "Demo E – Read asset: Sponsors (reads sponsor-info.md asset)",
    query: "Which sponsors have booths? Where can I find them?");

await RunDemoQuery(
    label: "Demo F – Multi-turn memory: References previous turn",
    query: "Going back to the sponsors — which one is running a prize draw and when?");

// ---------------------------------------------------------------------------
// 7. Interactive REPL for live audience questions.
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

    Console.WriteLine("\nConcierge:");
    await foreach (var chunk in agent.RunStreamingAsync(input, session))
        Console.Write(chunk);

    Console.WriteLine();
}

Console.WriteLine("\nThanks for attending Global Azure Bootcamp 2026! 🎉");

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

async Task RunDemoQuery(string label, string query)
{
    Console.WriteLine();
    Console.WriteLine($"┌─ {label}");
    Console.WriteLine($"│  Q: {query}");
    Console.WriteLine("│");
    Console.Write("│  A: ");

    await foreach (var chunk in agent.RunStreamingAsync(query, session))
        Console.Write(chunk);

    Console.WriteLine();
    Console.WriteLine("└─────────────────────────────────────────────────────");
}

void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
    Console.WriteLine("║   Global Azure Bootcamp 2026                          ║");
    Console.WriteLine("║   Demo 02: Microsoft Agent Framework – Agent Skills   ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("  Skills loaded from: " + Path.Combine(AppContext.BaseDirectory, "skills"));
    Console.WriteLine("  Inline skills: event-announcements (dynamic)");
    Console.WriteLine();
}
