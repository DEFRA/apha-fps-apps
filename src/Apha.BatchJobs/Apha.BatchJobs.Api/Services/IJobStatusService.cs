using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Provides job status information for the API layer.
/// Allows the UI to check if a job is currently running before triggering it.
/// </summary>
public interface IJobStatusService
{
    /// <summary>
    /// Gets the current status of a batch job, including whether a lock is held
    /// and the details of the last execution.
    /// </summary>
    /// <param name="jobName">Name of the batch job.</param>
    /// <param name="jobExecutionId">Optional execution id to correlate status for a specific trigger.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobStatusResult> GetStatusAsync(
        string jobName,
        Guid? jobExecutionId = null,
        DateTime? acceptedAtUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets status for a single execution by external execution id.
    /// </summary>
    /// <param name="jobExecutionId">External execution id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobStatusResult?> GetStatusByExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status for all registered batch jobs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<JobStatusResult>> GetAllStatusesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the current status of a batch job.
/// </summary>
public sealed class JobStatusResult
{
    /// <summary>Name of the batch job.</summary>
    public required string JobName { get; init; }

    /// <summary>
    /// Whether the job is currently running (lock is held and not expired).
    /// Use this to disable the trigger button in the UI.
    /// </summary>
    public required bool IsRunning { get; init; }

    /// <summary>
    /// The active lock, if held. Null if the job is not running.
    /// </summary>
    public ActiveLockInfo? ActiveLock { get; init; }

    /// <summary>
    /// Summary of the last completed execution, if any.
    /// </summary>
    public LastExecutionInfo? LastExecution { get; init; }

    /// <summary>
    /// Startup watchdog projection data for accepted triggers that are not yet observable as running.
    /// </summary>
    public StartupWatchdogInfo? StartupWatchdog { get; init; }

    /// <summary>
    /// Echoes the execution id used for status correlation, when available.
    /// </summary>
    public Guid? CorrelatedJobExecutionId { get; init; }

    /// <summary>
    /// Identifies which system is the authoritative source for run outcomes.
    /// </summary>
    public required string SourceOfTruth { get; init; }
}

/// <summary>Active lock information returned to the UI.</summary>
public sealed class ActiveLockInfo
{
    /// <summary>Unique job queue ID holding the lock.</summary>
    public required Guid JobQueueId { get; init; }

    /// <summary>When the lock was acquired.</summary>
    public required DateTime AcquiredAt { get; init; }

    /// <summary>When the lock will expire (safety timeout).</summary>
    public required DateTime ExpiresAt { get; init; }
}

/// <summary>Last execution summary returned to the UI.</summary>
public sealed class LastExecutionInfo
{
    /// <summary>Unique job queue ID for the last execution.</summary>
    public required Guid JobQueueId { get; init; }

    /// <summary>External execution ID for correlation.</summary>
    public required Guid JobExecutionId { get; init; }

    /// <summary>Status of the last execution (Completed, Failed, Skipped, etc).</summary>
    public required string Status { get; init; }

    /// <summary>When the last execution started.</summary>
    public required DateTime StartedAt { get; init; }

    /// <summary>When the last execution completed.</summary>
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Startup watchdog projection returned for trigger-to-start observability.
/// </summary>
public sealed class StartupWatchdogInfo
{
    /// <summary>
    /// State projected by startup watchdog policy.
    /// Expected values: TriggerAcceptedPendingStart, StartFailedTimeout.
    /// </summary>
    public required string ProjectedState { get; init; }

    /// <summary>
    /// Trigger acceptance time used as the projection baseline.
    /// </summary>
    public required DateTime AcceptedAtUtc { get; init; }

    /// <summary>
    /// Calculated startup deadline.
    /// </summary>
    public required DateTime StartupDeadlineUtc { get; init; }

    /// <summary>
    /// UTC timestamp when projection was evaluated.
    /// </summary>
    public required DateTime EvaluatedAtUtc { get; init; }

    /// <summary>
    /// Effective startup SLA in seconds.
    /// </summary>
    public required int StartupSlaSeconds { get; init; }

    /// <summary>
    /// Indicates if transport delivery exhaustion has been confirmed by transport-layer reconciler.
    /// </summary>
    public required bool DeliveryExhaustionConfirmed { get; init; }

    /// <summary>
    /// Owner system for delivery exhaustion status.
    /// </summary>
    public required string DeliveryExhaustionOwner { get; init; }
}
