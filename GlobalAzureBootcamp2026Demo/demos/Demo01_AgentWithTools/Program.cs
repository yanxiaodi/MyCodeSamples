// =============================================================================
// Global Azure Bootcamp 2026 – Demo 01: Agent with Tools
// =============================================================================
// This demo shows how to give a MAF AIAgent a set of function tools so it can
// answer real questions by calling your code — not just hallucinating answers.
//
// Use case: A conference assistant for Global Azure Bootcamp 2026 that knows
// the session schedule, speaker bios, and current time.
//
// Key concepts shown:
//   • Registering function tools with [Description] attributes
//   • Non-streaming agent invocation (RunAsync)
//   • Streaming agent invocation (RunStreamingAsync)
//   • Interactive REPL for live demos
// =============================================================================

using Demo01_AgentWithTools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using SharedConfig;

// ---------------------------------------------------------------------------
// 1. Build the Azure OpenAI client from user secrets.
//    The UserSecretsId in this project's .csproj matches AgentClientFactory.SharedUserSecretsId.
//    Run once (from any demo project) to configure:
//      dotnet user-secrets set "AzureOpenAI:Endpoint"       "https://<name>.openai.azure.com/"
//      dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
// ---------------------------------------------------------------------------
var (openAIClient, deploymentName) = AgentClientFactory.Create();

// ---------------------------------------------------------------------------
// 2. Create the agent and register the conference tools.
// ---------------------------------------------------------------------------
// GetChatClient returns OpenAI.Chat.ChatClient; the AsAIAgent() extension
// (from Microsoft.Agents.AI.OpenAI, namespace OpenAI.Chat) wraps it in a MAF AIAgent.
AIAgent agent = openAIClient
    .GetChatClient(deploymentName)
    .AsAIAgent(
        instructions: """
            You are the official assistant for Global Azure Bootcamp 2026.
            Help attendees navigate the conference: find sessions, look up speakers,
            check room details, and tell them what's on right now.
            Always be friendly, concise, and accurate.
            Use the provided tools to answer questions — do not make up session or speaker details.
            """,
        name: "GAB2026 Assistant",
        tools:
        [
            AIFunctionFactory.Create(ConferenceTools.GetSessionsByTrack),
            AIFunctionFactory.Create(ConferenceTools.GetSessionDetails),
            AIFunctionFactory.Create(ConferenceTools.GetSpeakerBio),
            AIFunctionFactory.Create(ConferenceTools.GetCurrentConferenceTime),
        ]);

PrintBanner();

// ---------------------------------------------------------------------------
// 3. Scripted demo queries — show non-streaming and streaming side by side.
// ---------------------------------------------------------------------------
await RunDemoQuery(
    label: "Demo A – Non-streaming: What's on in the AI track?",
    query: "What sessions are in the AI track?",
    streaming: false);

await RunDemoQuery(
    label: "Demo B – Streaming: Tell me about the MAF session and its speaker Alex Rivera.",
    query: "Give me details about the session 'Getting Started with Microsoft Agent Framework' " +
           "and tell me about the speaker Alex Rivera.",
    streaming: true);

await RunDemoQuery(
    label: "Demo C – Multi-tool: What should I attend right now?",
    query: "What time is it at the conference? Which sessions are starting soon in any track?",
    streaming: false);

// ---------------------------------------------------------------------------
// 4. Interactive REPL — great for live presenter questions.
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

    Console.WriteLine("\nAssistant (streaming):");
    await foreach (var chunk in agent.RunStreamingAsync(input))
        Console.Write(chunk);

    Console.WriteLine();
}

Console.WriteLine("\nThanks for attending Global Azure Bootcamp 2026! 🎉");

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

async Task RunDemoQuery(string label, string query, bool streaming)
{
    Console.WriteLine();
    Console.WriteLine($"┌─ {label}");
    Console.WriteLine($"│  Q: {query}");
    Console.WriteLine("│");
    Console.Write("│  A: ");

    if (streaming)
    {
        await foreach (var chunk in agent.RunStreamingAsync(query))
            Console.Write(chunk);
    }
    else
    {
        var result = await agent.RunAsync(query);
        Console.Write(result);
    }

    Console.WriteLine();
    Console.WriteLine("└─────────────────────────────────────────────────────");
}

void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
    Console.WriteLine("║   Global Azure Bootcamp 2026                          ║");
    Console.WriteLine("║   Demo 01: Microsoft Agent Framework – Agent + Tools  ║");
    Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
}
