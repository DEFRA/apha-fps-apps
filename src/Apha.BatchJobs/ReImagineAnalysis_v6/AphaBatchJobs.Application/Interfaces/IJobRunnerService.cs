namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Service interface for executing scheduled and ad-hoc batch jobs.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Executes all scheduled jobs that are due to run.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The number of jobs successfully executed.</returns>
    Task<int> RunScheduledJobsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes a specific job on-demand with provided parameters.
    /// </summary>
    /// <param name="jobName">The name of the job to execute.</param>
    /// <param name="parameters">Dictionary of parameters to pass to the job. Use IReadOnlyDictionary for better immutability.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The number of jobs successfully executed (typically 1 or 0).</returns>
    Task<int> RunAdhocJobAsync(string jobName, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken);
}

// Best Practice Notes:
// 1. Changed Dictionary<string, string> to IReadOnlyDictionary<string, string> for the parameters
//    - This prevents unintended modifications to the parameters collection
//    - Follows the principle of least privilege and immutability
//    - More appropriate for method parameters that should not be modified by the implementation
// 
// 2. Added XML documentation comments for better IntelliSense support and API documentation
//    - Helps developers understand the purpose and usage of each method
//    - Standard practice for public interfaces in .NET
//
// 3. Maintained existing functionality without adding new features
// 
// 4. Consider for future improvements (not implemented to preserve existing functionality):
//    - Return a Result<T> or custom response object instead of int for better error handling
//    - Add overload with default parameters = null for RunAdhocJobAsync
//    - Consider using ValueTask<int> for potentially synchronous completions