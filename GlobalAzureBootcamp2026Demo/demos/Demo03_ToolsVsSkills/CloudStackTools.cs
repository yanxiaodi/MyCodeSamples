// =============================================================================
// CloudStack fake tools — represent live, stateful, or side-effecting operations
// that CANNOT live in a static knowledge file.
// =============================================================================
// Tools are the right choice when:
//   • The answer changes in real time (service health, account data)
//   • The action has a side effect (create ticket, send email, call API)
//   • The data is per-user / per-session (you can't pre-write it into a skill)
// =============================================================================

using System.ComponentModel;

namespace Demo03_ToolsVsSkills;

internal static class CloudStackTools
{
    [Description(
        "Get the current health status of a CloudStack service. " +
        "Returns live status, uptime percentage, and any active incidents. " +
        "Use for any question about whether a service is up, slow, or degraded.")]
    public static ServiceHealth GetServiceHealth(
        [Description("Service name: 'api', 'auth', 'storage', 'compute', or 'all'")] string service)
    {
        // Simulates a live status-page API response — the answer changes minute to minute,
        // so it fundamentally cannot come from a static SKILL.md file.
        return service.ToLowerInvariant() switch
        {
            "api"     => new("api",     "degraded", 99.2,  "Elevated latency on POST /v2/jobs — p95 latency 4.2 s (SLA: 1 s). Engineers investigating."),
            "auth"    => new("auth",    "healthy",  99.98, null),
            "storage" => new("storage", "healthy",  99.99, null),
            "compute" => new("compute", "healthy",  99.95, null),
            _         => new("all",     "degraded", 99.6,  "API service experiencing elevated latency. All other services healthy."),
        };
    }

    [Description(
        "Look up a CloudStack customer account by email address. " +
        "Returns account details: name, current plan, member since, API call usage, and billing status. " +
        "Use when a customer asks about their account, plan, or usage.")]
    public static CustomerAccount? GetCustomerAccount(
        [Description("The customer's email address")] string email)
    {
        // Per-user data — completely impossible to put into a skill file ahead of time.
        var accounts = new Dictionary<string, CustomerAccount>(StringComparer.OrdinalIgnoreCase)
        {
            ["alice@contoso.com"]   = new("alice@contoso.com",   "Alice Chen",   "Free",       "2024-06-15", 82,   "current"),
            ["bob@fabrikam.com"]    = new("bob@fabrikam.com",    "Bob Martinez", "Pro",        "2023-11-01", 3_402, "current"),
            ["carol@northwind.com"] = new("carol@northwind.com", "Carol Singh",  "Enterprise", "2022-03-20", 120,  "current"),
            ["dave@tailspin.com"]   = new("dave@tailspin.com",   "Dave O'Brien", "Starter",    "2025-01-10", 1_908, "overdue"),
        };

        return accounts.TryGetValue(email, out var account) ? account : null;
    }

    [Description(
        "Create a new CloudStack support ticket. " +
        "Returns the new ticket ID, status, and confirmation. " +
        "Use when a customer explicitly asks to open, raise, or log a support ticket.")]
    public static TicketResult CreateSupportTicket(
        [Description("Short title for the ticket (max 80 characters)")] string title,
        [Description("Detailed description of the issue or request")] string description,
        [Description("Priority level: 'low', 'normal', 'high', or 'critical'")] string priority)
    {
        // A side effect — creates a record in a (fake) ticketing system.
        // No skill can do this: skills provide knowledge; they don't write to systems.
        var ticketId = $"CS-{Random.Shared.Next(10_000, 99_999)}";
        var eta = priority.ToLowerInvariant() switch
        {
            "critical" => "2 hours",
            "high"     => "4 hours",
            "normal"   => "1 business day",
            _          => "3 business days",
        };
        return new(ticketId, title, priority, "open",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), eta);
    }
}

// ---------------------------------------------------------------------------
// Record types returned by the tools — the agent sees these as JSON.
// ---------------------------------------------------------------------------

internal record ServiceHealth(
    string Service,
    string Status,
    double UptimePercent,
    string? ActiveIncident);

internal record CustomerAccount(
    string Email,
    string Name,
    string Plan,
    string MemberSince,
    int ApiCallsThisMonth,
    string BillingStatus);

internal record TicketResult(
    string TicketId,
    string Title,
    string Priority,
    string Status,
    string CreatedAt,
    string ExpectedResponseTime);
