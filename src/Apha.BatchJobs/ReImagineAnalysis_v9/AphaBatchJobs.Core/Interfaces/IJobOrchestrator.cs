namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Defines the contract for job orchestration, coordinating the execution of batch jobs.
/// </summary>
public interface IJobOrchestrator
{
    /// <summary>
    /// Orchestrates the execution of a batch job asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing job metadata and correlation information.</param>
    /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the job execution result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    Task<Models.JobExecutionResult> ExecuteAsync(
        Models.JobExecutionContext context, 
        CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default value for CancellationToken parameter (= default) - .NET 8 best practice for async methods
// 2. Added XML documentation for exceptions that implementers should throw - improves API contract clarity
// 3. Formatted method parameters on separate lines for better readability when parameters have lengthy types
// 4. Added exception documentation for ArgumentNullException and OperationCanceledException which are standard for async operations
// 5. Maintained all existing functionality without adding new features