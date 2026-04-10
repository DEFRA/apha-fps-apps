namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Contract for job orchestrators that coordinate complex job execution workflows.
/// Orchestrators manage the execution lifecycle, error handling, and result aggregation
/// for batch jobs that may involve multiple steps or operations.
/// </summary>
public interface IJobOrchestrator
{
    /// <summary>
    /// Orchestrates the execution of a job with the provided context and cancellation support.
    /// </summary>
    /// <param name="context">The execution context containing job metadata and tracking information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during execution.</param>
    /// <returns>A task representing the asynchronous operation, containing the job execution result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    Task<Models.JobExecutionResult> ExecuteAsync(
        Models.JobExecutionContext context, 
        CancellationToken cancellationToken = default);
}


// Review Comments:
// 1. Added default value for CancellationToken parameter (= default) - this is a .NET best practice
//    to make the cancellation token optional while maintaining the async pattern
// 2. Added XML documentation for exceptions that implementers should throw - improves API clarity
// 3. Interface structure follows .NET naming conventions and async patterns correctly
// 4. The interface is appropriately minimal and focused on a single responsibility
// 5. Return type uses fully qualified namespace (Models.) which is acceptable for interface definitions
// 6. Consider using fully qualified type names (AphaBatchJobs.Core.Models.JobExecutionResult) 
//    if Models namespace might be ambiguous, but current approach is acceptable