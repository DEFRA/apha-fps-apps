using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Repository for managing job execution records and history.
/// </summary>
public interface IJobExecutionRepository
{
    /// <summary>
    /// Records the start of a job execution.
    /// </summary>
    /// <param name="record">The execution record to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the created execution record.</returns>
    Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing execution record.
    /// </summary>
    /// <param name="record">The execution record to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last execution record for a given job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an execution record by its external execution identifier.
    /// </summary>
    /// <param name="jobExecutionId">External job execution id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobExecutionRecord?> GetExecutionByJobExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to persist an idempotent cancellation request keyed by job execution id.
    /// </summary>
    /// <param name="jobExecutionId">External job execution id.</param>
    /// <param name="requestedBy">Identity that requested cancellation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// True when cancellation is newly requested; false when an existing request is already stored.
    /// </returns>
    Task<bool> TryRequestCancellationAsync(Guid jobExecutionId, string requestedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when a cancellation request exists for the given execution id.
    /// </summary>
    /// <param name="jobExecutionId">External job execution id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> IsCancellationRequestedAsync(Guid jobExecutionId, CancellationToken cancellationToken = default);
}
