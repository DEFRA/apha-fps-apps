namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Contract for adhoc jobs that run on demand triggered by CLI arguments.
/// Adhoc jobs are identified by a unique JobName and executed via the --adhoc command line parameter.
/// </summary>
public interface IAdhocJob
{
    /// <summary>
    /// Gets the unique name of the adhoc job used for lookup and execution.
    /// This name is matched against the job name parameter passed via CLI.
    /// </summary>
    string JobName { get; }

    /// <summary>
    /// Executes the adhoc job with the provided execution context and cancellation support.
    /// </summary>
    /// <param name="context">The execution context containing job metadata and tracking information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns the job execution result.</returns>
    Task<Models.JobExecutionResult> ExecuteAsync(
        Models.JobExecutionContext context, 
        CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default parameter value for cancellationToken (= default) to follow .NET best practices
//    This allows callers to omit the cancellation token if not needed while still supporting cancellation
// 2. Formatted method parameters on separate lines for better readability (common in .NET conventions)
// 3. All other aspects of the interface are well-designed and follow .NET naming conventions and documentation standards