namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Service contract for orchestrating job execution.
/// Provides methods to run scheduled jobs and adhoc jobs on demand.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Runs all registered scheduled jobs sequentially.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Integer exit code indicating the overall execution result.</returns>
    Task<int> RunScheduledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a specific adhoc job identified by its name.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Integer exit code indicating the execution result.</returns>
    Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default);
}

// Review Notes:
// 1. Added default parameter value for CancellationToken (= default) to follow .NET best practices
//    This makes the API more flexible and easier to use when cancellation is not needed
// 2. The interface is well-structured with clear XML documentation
// 3. Method naming follows async suffix convention correctly
// 4. Return type of int for exit codes is appropriate for batch job scenarios
// 5. Consider validating jobName parameter is not null/empty in the implementation
// 6. The interface follows single responsibility principle focusing on job execution orchestration