namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Service interface for running batch jobs in scheduled or adhoc mode.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Runs all registered scheduled jobs sequentially.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop execution.</param>
    /// <returns>Exit code indicating overall execution status.</returns>
    Task<int> RunScheduledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a specific adhoc job by name.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to stop execution.</param>
    /// <returns>Exit code indicating job execution status.</returns>
    Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default);
}
