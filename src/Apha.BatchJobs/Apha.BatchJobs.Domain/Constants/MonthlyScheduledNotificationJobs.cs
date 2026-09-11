namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Jobs that fire together, in one worker invocation, when EventBridge sends the shared
/// category trigger name <see cref="EventBridgeCategoryName"/>. Each is also treated as
/// worker-managed (self-generated execution ID) for Scheduled runs. Add a job's canonical
/// name here to include it in the category — no other resolver or orchestrator code changes.
/// </summary>
public static class MonthlyScheduledNotificationJobs
{
    public const string EventBridgeCategoryName = "MonthlyBusinessNotifications";

    public static readonly IReadOnlyList<string> JobNames =
    [
        BatchJobNames.MilestoneUpdateNotifications,
    ];

    public static bool Contains(string jobName) =>
        JobNames.Any(name => string.Equals(name, jobName, StringComparison.OrdinalIgnoreCase));
}
