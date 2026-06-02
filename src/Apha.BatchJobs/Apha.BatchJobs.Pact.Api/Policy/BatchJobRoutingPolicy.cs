namespace Apha.BatchJobs.Pact.Api.Policy;

public enum TriggerApiSource
{
    Fps,
    Pact
}

public enum JobRouteKind
{
    FpsApi,
    PactApi,
    ScheduledOnly,
    Neutral
}

public sealed record BatchJobRoute(string JobName, JobRouteKind RouteKind, string Description);

public static class BatchJobRoutingPolicy
{
    private static readonly IReadOnlyList<BatchJobRoute> Routes =
    [
        new("RecreateSummaries", JobRouteKind.PactApi, "Mapped to PACT API"),
        new("MABArchive", JobRouteKind.ScheduledOnly, "Scheduled job only; year is derived internally from execution date"),
        new("FECProcess", JobRouteKind.FpsApi, "Mapped to FPS API"),
        new("YearEndProcess", JobRouteKind.FpsApi, "Mapped to FPS API"),
        new("HealthCheck", JobRouteKind.Neutral, "Neutral health trigger; allowed via FPS or PACT API")
    ];

    public static IReadOnlyList<BatchJobRoute> GetCatalog() => Routes;

    public static bool TryResolveRoute(string? jobName, out BatchJobRoute? route, out string? error)
    {
        route = null;
        error = null;

        if (string.IsNullOrWhiteSpace(jobName))
        {
            error = "Job name is required.";
            return false;
        }

        var normalized = NormalizeJobName(jobName);
        route = Routes.FirstOrDefault(r => r.JobName.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (route is null)
        {
            error = $"Job '{jobName}' is not recognized by the trigger routing policy.";
            return false;
        }

        return true;
    }

    public static bool CanTriggerFromSource(
        string? jobName,
        TriggerApiSource source,
        out string normalizedJobName,
        out string? reason)
    {
        normalizedJobName = string.Empty;
        reason = null;

        if (!TryResolveRoute(jobName, out var route, out reason) || route is null)
            return false;

        normalizedJobName = route.JobName;

        if (route.RouteKind == JobRouteKind.ScheduledOnly)
        {
            reason = $"Job '{route.JobName}' is scheduled-only and cannot be triggered by API.";
            return false;
        }

        if (route.RouteKind == JobRouteKind.Neutral)
            return true;

        var allowedSource = route.RouteKind == JobRouteKind.FpsApi ? TriggerApiSource.Fps : TriggerApiSource.Pact;
        if (source != allowedSource)
        {
            reason = $"Job '{route.JobName}' is mapped to {allowedSource} API and cannot be triggered from {source} API.";
            return false;
        }

        return true;
    }

    private static string NormalizeJobName(string input)
    {
        if (input.Equals("YearEndProces", StringComparison.OrdinalIgnoreCase))
            return "YearEndProcess";

        return input.Trim();
    }
}
