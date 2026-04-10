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
    /// Retrieves an execution record by ID.
    /// </summary>
    /// <param name="executionId">The execution record ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobExecutionRecord?> GetExecutionRecordAsync(int executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last execution record for a given job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default);
}
