namespace Apha.BatchJobs.Domain.Entities;

/// <summary>A dispatched job_queue row that no worker claimed within its time to live, and was marked Failed.</summary>
public sealed record StaleDispatchedExecution(
    Guid JobQueueId,
    Guid JobExecutionId,
    DateTime? DispatchedAtUtc,
    int TimeToLiveMinutes);
