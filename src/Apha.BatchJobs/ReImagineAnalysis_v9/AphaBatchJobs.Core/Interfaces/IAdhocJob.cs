namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Defines the contract for adhoc jobs that run on demand with a specific job name identifier.
/// Adhoc jobs are triggered manually via CLI arguments and execute specific batch operations.
/// </summary>
public interface IAdhocJob
{
    /// <summary>
    /// Gets the unique name that identifies this adhoc job.
    /// This name is used for lookup and execution when triggered via CLI.
    /// </summary>
    string JobName { get; }

    /// <summary>
    /// Executes the adhoc job asynchronously with the provided execution context.
    /// </summary>
    /// <param name="context">The execution context containing job metadata and correlation information.</param>
    /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the job execution result with status, message, and exit code.</returns>
    Task<Models.JobExecutionResult> ExecuteAsync(Models.JobExecutionContext context, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default value for cancellationToken parameter (= default) to follow .NET 8 best practices
//    This makes the interface more flexible and allows callers to omit the cancellation token if not needed
//    while still maintaining the ability to pass one when required for proper async operation cancellation.
//
// Note: The code is already well-structured with proper XML documentation and follows .NET naming conventions.
// The interface is appropriately minimal and focused on its single responsibility.