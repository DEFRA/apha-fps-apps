using AphaBatchJobsWave2.AdhocRecreateSummaries.Models;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Interfaces
{
    /// <summary>
    /// Defines the contract for adhoc jobs that can be executed on-demand.
    /// Adhoc jobs are typically long-running operations that perform batch processing,
    /// data transformations, or system maintenance tasks.
    /// </summary>
    /// <remarks>
    /// Implementations must:
    /// - Execute within the specified timeout period (300 seconds per step)
    /// - Return appropriate exit codes (0=success, 1=failure, 2=timeout)
    /// - Log execution details including correlation IDs
    /// - Handle failures gracefully and stop on first error
    /// - Preserve transactional integrity where applicable
    /// </remarks>
    public interface IAdhocJob
    {
        /// <summary>
        /// Executes the adhoc job asynchronously for the specified month.
        /// </summary>
        /// <param name="month">
        /// The month number (1-12) for which the job should be executed.
        /// This parameter is used to scope the job execution to a specific reporting period.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// This enables graceful shutdown and timeout handling in ECS Fargate environments.
        /// </param>
        /// <returns>
        /// A <see cref="Task{JobExecutionResult}"/> representing the asynchronous operation.
        /// The result contains:
        /// - Status: "Success", "Failure", or "Timeout"
        /// - Message: Detailed information about the execution outcome
        /// - ExitCode: 0 for success, 1 for failure, 2 for timeout
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the month parameter is not in the valid range (1-12).
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is canceled via the cancellation token.
        /// </exception>
        /// <remarks>
        /// Implementations should:
        /// - Validate the month parameter (1-12 range)
        /// - Execute all required steps in strict order
        /// - Stop immediately on first failure
        /// - Enforce timeout constraints per step
        /// - Log step start, end, duration, and correlation ID
        /// - Return appropriate JobExecutionResult based on outcome
        /// - Respect the cancellation token for graceful shutdown
        /// </remarks>
        Task<JobExecutionResult> ExecuteAsync(int month, CancellationToken cancellationToken = default);
    }
}


// Key improvements made:
// 1. Added CancellationToken parameter with default value for backward compatibility
//    - Essential for ECS Fargate graceful shutdown (SIGTERM handling)
//    - Enables proper timeout handling in Quartz jobs
//    - Follows .NET async best practices
//
// 2. Added explicit exception documentation
//    - ArgumentOutOfRangeException for invalid month values
//    - OperationCanceledException for cancellation scenarios
//    - Improves API contract clarity
//
// 3. Updated remarks to include cancellation token usage
//    - Emphasizes graceful shutdown requirements
//    - Aligns with ECS Fargate container lifecycle management
//
// 4. Maintained all existing functionality and documentation
//    - No new features added
//    - Preserved original intent and design
//    - Enhanced for production-ready async patterns