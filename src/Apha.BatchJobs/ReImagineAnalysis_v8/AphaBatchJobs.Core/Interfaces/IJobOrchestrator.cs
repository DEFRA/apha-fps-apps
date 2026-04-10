namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for job orchestration within the Apha batch processing system.
/// Implementers of this interface are responsible for coordinating the execution of batch jobs,
/// managing their lifecycle, and returning execution results.
/// </summary>
/// <remarks>
/// The IJobOrchestrator serves as a central coordination point for job execution,
/// allowing for consistent handling of job lifecycle events, error management,
/// and result reporting across different job types (scheduled and adhoc).
/// </remarks>
public interface IJobOrchestrator
{
    /// <summary>
    /// Orchestrates the execution of a batch job asynchronously.
    /// This method coordinates all aspects of job execution including initialization,
    /// execution, error handling, and result reporting.
    /// </summary>
    /// <param name="context">
    /// The execution context containing job metadata such as job name, correlation ID,
    /// trigger type (Scheduled or Adhoc), and start timestamp. This context is used
    /// for tracking and logging throughout the job execution lifecycle.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the job execution.
    /// Implementers should monitor this token and gracefully terminate execution
    /// when cancellation is requested.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="JobExecutionResult"/> with the execution status, message, and exit code.
    /// Exit code 0 typically indicates successful execution, while non-zero values
    /// indicate various failure conditions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the context parameter is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellation token.
    /// </exception>
    Task<JobExecutionResult> ExecuteAsync(
        JobExecutionContext context, 
        CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default value for cancellationToken parameter (= default) - This is a .NET best practice
//    that makes the API more flexible and easier to use when cancellation is not needed
// 2. Formatted method parameters on separate lines for better readability when parameters have extensive documentation
// 3. All other aspects of the interface remain unchanged as they follow proper .NET conventions:
//    - Proper XML documentation
//    - Async suffix on async method
//    - CancellationToken as last parameter
//    - Appropriate return type (Task<T>)
//    - Clear interface naming with 'I' prefix