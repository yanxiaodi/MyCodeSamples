using System.ComponentModel;

namespace Demo01_AgentWithTools;

/// <summary>
/// Function tools exposed to the MAF agent.
/// Each method is decorated with [Description] so the LLM knows when and how to call it.
/// </summary>
public static class ConferenceTools
{
    // Static in-memory data that simulates a conference database.
    private static readonly Dictionary<string, List<string>> SessionsByTrack = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AI"] =
        [
            "Getting Started with Microsoft Agent Framework",
            "Building Multi-Agent Systems with MAF",
            "Semantic Kernel vs Agent Framework: Choose Wisely",
            "Real-World RAG: Lessons Learned",
        ],
        ["Cloud"] =
        [
            "Azure Container Apps: Production Patterns",
            "Cost Optimisation on Azure in 2026",
            "Multi-Region Active-Active Architectures",
        ],
        ["DevOps"] =
        [
            "GitHub Copilot for Platform Engineers",
            "GitOps at Scale with Flux",
            "Shift-Left Security in CI/CD Pipelines",
        ],
        ["Security"] =
        [
            "Zero Trust in Practice",
            "Entra ID: Beyond the Basics",
            "AI Red-Teaming Your Own Apps",
        ],
    };

    private static readonly Dictionary<string, (string Room, string Time, string Speaker, string Abstract)> SessionDetails = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Getting Started with Microsoft Agent Framework"] =
            ("Hall A", "09:00 – 09:50", "Alex Rivera",
             "A hands-on introduction to MAF: create your first agent, add tools, handle multi-turn " +
             "conversations, and host it as an API — all in under an hour."),

        ["Building Multi-Agent Systems with MAF"] =
            ("Hall A", "10:00 – 10:50", "Jordan Lee",
             "Deep-dive into orchestrating multiple specialised agents with handoff and group-chat patterns."),

        ["Semantic Kernel vs Agent Framework: Choose Wisely"] =
            ("Room 12", "11:00 – 11:50", "Jordan Lee",
             "Side-by-side comparison of SK and MAF to help you pick the right tool for your scenario."),

        ["Real-World RAG: Lessons Learned"] =
            ("Room 12", "14:00 – 14:50", "Sam Patel",
             "Practical advice from production RAG deployments: chunking, re-ranking, and evaluation."),

        ["Azure Container Apps: Production Patterns"] =
            ("Hall B", "09:00 – 09:50", "Sam Patel",
             "Scale-to-zero, KEDA, Dapr sidecars, and traffic splitting in ACA."),

        ["Cost Optimisation on Azure in 2026"] =
            ("Hall B", "10:00 – 10:50", "Sam Patel",
             "Spot instances, reserved capacity, auto-shutdown policies, and FinOps tooling."),

        ["Multi-Region Active-Active Architectures"] =
            ("Hall B", "14:00 – 14:50", "Alex Rivera",
             "Traffic Manager, Front Door, and Cosmos DB global distribution patterns."),

        ["GitHub Copilot for Platform Engineers"] =
            ("Room 7", "09:00 – 09:50", "Morgan Chen",
             "Using Copilot to generate IaC, write pipeline YAML, and document your platform."),

        ["GitOps at Scale with Flux"] =
            ("Room 7", "10:00 – 10:50", "Morgan Chen",
             "Managing hundreds of clusters with Flux, tenancy, and progressive delivery."),

        ["Shift-Left Security in CI/CD Pipelines"] =
            ("Room 7", "13:00 – 13:50", "Morgan Chen",
             "SAST, DAST, and secret scanning baked into every PR — without slowing teams down."),

        ["Zero Trust in Practice"] =
            ("Room 3", "09:00 – 09:50", "Taylor Kim",
             "Implementing Zero Trust principles across identity, network, and data layers on Azure."),

        ["Entra ID: Beyond the Basics"] =
            ("Room 3", "11:00 – 11:50", "Taylor Kim",
             "Conditional access, Privileged Identity Management, and Workload Identities."),

        ["AI Red-Teaming Your Own Apps"] =
            ("Room 3", "14:00 – 14:50", "Taylor Kim",
             "Prompt injection, jailbreaks, and how to systematically test your AI applications."),
    };

    private static readonly Dictionary<string, (string Company, string Bio)> Speakers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Alex Rivera"] = ("Contoso", "Principal engineer at Contoso with 15 years in distributed systems. " +
                                       "Core contributor to Microsoft Agent Framework."),
        ["Sam Patel"]   = ("Fabrikam", "Cloud architect at Fabrikam specialising in Azure-native SaaS. " +
                                        "Frequent speaker at Azure community events."),
        ["Jordan Lee"]  = ("Microsoft", "Senior PM on the Azure AI Platform team. " +
                                         "Passionate about making AI accessible to every developer."),
        ["Morgan Chen"] = ("Northwind", "DevOps lead at Northwind. GitHub Star and open-source maintainer."),
        ["Taylor Kim"]  = ("Independent", "Security researcher and AI red-teamer. " +
                                           "Author of 'Securing Your AI Pipeline'."),
    };

    // -------------------------------------------------------------------------
    // Tools
    // -------------------------------------------------------------------------

    [Description("Returns the list of sessions in a given conference track. " +
                 "Valid tracks are: AI, Cloud, DevOps, Security.")]
    public static string GetSessionsByTrack(
        [Description("The track name. One of: AI, Cloud, DevOps, Security.")] string track)
    {
        if (!SessionsByTrack.TryGetValue(track, out var sessions))
            return $"Track '{track}' not found. Available tracks: {string.Join(", ", SessionsByTrack.Keys)}.";

        var lines = sessions.Select((title, i) =>
        {
            var speaker = SessionDetails.TryGetValue(title, out var d) ? d.Speaker : "TBA";
            return $"  {i + 1}. {title} — {speaker}";
        });

        return $"Sessions in the {track} track:\n" + string.Join("\n", lines);
    }

    [Description("Returns the room, time slot, speaker, and abstract for a specific session.")]
    public static string GetSessionDetails(
        [Description("The exact session title.")] string sessionTitle)
    {
        if (!SessionDetails.TryGetValue(sessionTitle, out var details))
            return $"Session '{sessionTitle}' not found. Check the title and try again.";

        return $"Session:  {sessionTitle}\n" +
               $"Speaker:  {details.Speaker}\n" +
               $"Room:     {details.Room}\n" +
               $"Time:     {details.Time}\n" +
               $"Abstract: {details.Abstract}";
    }

    [Description("Returns the bio and company of a conference speaker.")]
    public static string GetSpeakerBio(
        [Description("The full name of the speaker.")] string name)
    {
        if (!Speakers.TryGetValue(name, out var info))
            return $"Speaker '{name}' not found in the roster. " +
                   $"Known speakers: {string.Join(", ", Speakers.Keys)}.";

        return $"{name} — {info.Company}\n{info.Bio}";
    }

    [Description("Returns the current simulated conference time so the agent can tell " +
                 "attendees which sessions are happening now or coming up next.")]
    public static string GetCurrentConferenceTime()
    {
        // Simulate the conference being mid-morning on the event day.
        return "Current conference time: 09:55 AM, Saturday 18 April 2026.";
    }
}
