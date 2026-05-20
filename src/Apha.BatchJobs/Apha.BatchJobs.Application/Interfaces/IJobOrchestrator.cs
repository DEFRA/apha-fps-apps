using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Orchestrates the full lifecycle of a batch job execution:
/// acquire lock → record start → run job → record result → release lock.
/// </summary>
public interface IJobOrchestrator
{
    /// <summary>
    /// Runs a named job through the full execution lifecycle.
    /// </summary>
    /// <param name="jobName">The registered name of the job to run.</param>
    /// <param name="runMode">Whether this is a scheduled or ad-hoc run.</param>
    /// <param name="jobQueueId">The caller-injected UUID for this job queue entry (strict mode - always required).</param>
    /// <param name="userId">The user/system ID requesting the job execution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result including JobQueueId, status, and duration.</returns>
    Task<JobExecutionResult> RunAsync(string jobName, RunMode runMode, Guid jobQueueId, string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Holds the outcome of a <see cref="IJobOrchestrator.RunAsync"/> call.
/// </summary>
public sealed record JobExecutionResult(
    Guid JobQueueId,
    string JobName,
    JobStatus Status,
    TimeSpan Duration,
    int? ExecutionId = null,
    string? ErrorMessage = null
);
