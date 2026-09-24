namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Maps a <see cref="BatchFailureCategory"/> to a business/user-facing friendly message. Shared by
/// <c>Apha.BatchJobs.Worker.Reporting.BatchRunSummaryWriter</c> (CloudWatch run-summary line) and
/// <c>Orchestration.JobOrchestrator</c> (persisted <c>job_queue.errormessage</c>) so the same
/// failure always produces the same friendly text in both places, from one definition.
/// </summary>
public static class BatchFailureMessageProvider
{
    public static string GetHumanReadableMessage(BatchFailureCategory category) => category switch
    {
        BatchFailureCategory.Sql =>
            "Job failed due to a SQL error.",
        BatchFailureCategory.DependencyOutage =>
            "Job failed due to a dependency outage (database unavailable, network timeout, etc.).",
        BatchFailureCategory.Configuration =>
            "Job failed due to a configuration or validation error.",
        BatchFailureCategory.Concurrency =>
            "Job failed because the distributed lock could not be acquired.",
        BatchFailureCategory.LockLeaseLost =>
            "Job failed because it lost its distributed lock lease mid-execution.",
        BatchFailureCategory.Email =>
            "Job failed due to a business notification email error.",
        BatchFailureCategory.Timeout =>
            "Job failed because execution exceeded the configured runtime timeout.",
        BatchFailureCategory.Authorization =>
            "Job failed due to an authorization error.",
        _ => "Job failed with a business or runtime exception."
    };
}
