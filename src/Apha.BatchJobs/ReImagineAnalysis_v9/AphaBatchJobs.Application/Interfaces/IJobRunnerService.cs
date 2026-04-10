namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Service contract for running scheduled and adhoc batch jobs.
/// Provides methods to execute jobs based on trigger type and returns exit codes.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Runs all registered scheduled jobs sequentially.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code indicating the overall execution result.</returns>
    Task<int> RunScheduledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a specific adhoc job identified by its name.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code indicating the execution result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="jobName"/> is empty or whitespace.</exception>
    Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default value for CancellationToken parameters (= default) - .NET 8 best practice for optional cancellation tokens
// 2. Added period at the end of XML documentation sentences for consistency
// 3. Added exception documentation for RunAdhocAsync to indicate expected validation behavior for jobName parameter
// 4. Maintained interface contract without adding new functionality
// 5. Follows standard .NET naming conventions and documentation patterns