// ============================================================================
// File: IScheduledJob.cs
// Description: Interface contract for scheduled job execution with async 
//              execution method and job identification for Apha BatchJobs Foundation
// ============================================================================

using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundation.Core.Models;

namespace AphaBatchJobsFoundation.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for scheduled batch jobs in the Apha BatchJobs system.
    /// Scheduled jobs are executed on a recurring basis according to a defined schedule
    /// (e.g., daily, hourly, cron expression) and are managed by the job orchestrator.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should:
    /// - Be stateless to support concurrent execution if needed
    /// - Handle cancellation gracefully via the CancellationToken
    /// - Return appropriate JobExecutionResult with status and exit codes
    /// - Log execution details using the correlation ID from JobExecutionContext
    /// - Delegate business logic to service layer components
    /// - Not contain infrastructure concerns (database connections, file I/O setup, etc.)
    /// </remarks>
    public interface IScheduledJob
    {
        /// <summary>
        /// Gets the unique name identifier for the scheduled job.
        /// This name is used for job registration, scheduling configuration,
        /// logging, and monitoring purposes.
        /// </summary>
        /// <remarks>
        /// The job name should:
        /// - Be unique across all scheduled jobs in the system
        /// - Follow naming conventions (e.g., PascalCase)
        /// - Be descriptive of the job's purpose
        /// - Remain consistent across deployments for scheduler configuration
        /// </remarks>
        /// <example>
        /// Examples of valid job names:
        /// - "DailyOrderProcessing"
        /// - "HourlyInventorySync"
        /// - "MonthlyReportGeneration"
        /// </example>
        string JobName { get; }

        /// <summary>
        /// Executes the scheduled job asynchronously with the provided execution context.
        /// This method is called by the job orchestrator when the job is triggered
        /// according to its schedule.
        /// </summary>
        /// <param name="context">
        /// The job execution context containing correlation ID, job parameters,
        /// execution metadata, and other contextual information required for job execution.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to observe for graceful shutdown requests.
        /// Implementations should check this token periodically during long-running
        /// operations and return a cancelled result when cancellation is requested.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with:
        /// - Status: Completed, Failed, or Cancelled
        /// - ExitCode: 0 for success, non-zero for failures
        /// - Message: Execution summary or error details
        /// - CompletedAt: Timestamp of completion
        /// - Exception: Exception details if the job failed
        /// </returns>
        /// <remarks>
        /// Implementation guidelines:
        /// - Use async/await for all I/O and database operations
        /// - Propagate the CancellationToken to all async operations
        /// - Use structured logging with the correlation ID from context
        /// - Return JobExecutionResult.Success() for successful completion
        /// - Return JobExecutionResult.Failure() with details for errors
        /// - Return JobExecutionResult.Cancelled() when cancellation is detected
        /// - Handle exceptions gracefully and include them in the result
        /// - Keep orchestration logic minimal and delegate to services
        /// - Ensure proper disposal of database connections and resources
        /// - Use parameterized queries to prevent SQL injection
        /// - Implement retry logic for transient database failures
        /// - Use transactions appropriately for data consistency
        /// </remarks>
        /// <example>
        /// <code>
        /// public async Task&lt;JobExecutionResult&gt; ExecuteAsync(
        ///     JobExecutionContext context, 
        ///     CancellationToken cancellationToken)
        /// {
        ///     try
        ///     {
        ///         _logger.LogInformation(
        ///             "Starting job {JobName} with correlation ID {CorrelationId}",
        ///             context.JobName,
        ///             context.CorrelationId);
        ///         
        ///         await _businessService.ProcessDataAsync(cancellationToken);
        ///         
        ///         return JobExecutionResult.Success("Job completed successfully");
        ///     }
        ///     catch (OperationCanceledException)
        ///     {
        ///         return JobExecutionResult.Cancelled();
        ///     }
        ///     catch (Exception ex)
        ///     {
        ///         _logger.LogError(ex, "Job execution failed");
        ///         return JobExecutionResult.Failure("Job execution failed", ex);
        ///     }
        /// }
        /// </code>
        /// </example>
        Task<JobExecutionResult> ExecuteAsync(
            JobExecutionContext context, 
            CancellationToken cancellationToken);
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Design Decisions:
// 1. Interface defines contract for scheduled jobs only (adhoc jobs will have
//    separate interface if needed based on requirements)
// 2. JobName as property rather than method for cleaner syntax and consistency
//    with C# property conventions
// 3. ExecuteAsync follows async/await pattern with CancellationToken support
//    for graceful shutdown and timeout handling
// 4. Returns Task<JobExecutionResult> for comprehensive execution outcome
//    including status, exit codes, and error details
// 5. JobExecutionContext parameter provides all necessary execution metadata
//    without coupling to specific parameter types
//
// Architecture Alignment:
// - Interface-based design supports dependency injection and testability
// - Async signature supports scalable I/O operations
// - Clean separation between orchestration (this interface) and business logic
// - Context object pattern allows extensibility without breaking changes
// - CancellationToken enables cooperative cancellation for long-running jobs
//
// Scheduler Integration:
// - JobName used for job registration and configuration mapping
// - JobExecutionResult.ExitCode maps to process exit codes for scheduler
// - Async execution supports non-blocking job orchestration
// - CancellationToken enables timeout enforcement by scheduler
//
// Usage Pattern:
// 1. Implement this interface for each scheduled job
// 2. Register implementation in DI container
// 3. Job orchestrator discovers and schedules registered jobs
// 4. Scheduler triggers ExecuteAsync at configured intervals
// 5. Result returned to orchestrator for logging and exit code handling
//
// Testing Considerations:
// - Interface enables easy mocking for unit tests
// - JobExecutionContext can be constructed with test data
// - CancellationToken can be triggered in tests to verify cancellation handling
// - JobExecutionResult factory methods simplify test assertions
//
// Thread Safety:
// - Implementations should be stateless or thread-safe
// - Multiple instances may execute concurrently if scheduler allows
// - Context object is per-execution and should not be shared
//
// Error Handling:
// - Implementations should catch exceptions and return Failure result
// - Unhandled exceptions will be caught by orchestrator
// - OperationCanceledException should result in Cancelled status
// - Exit codes should follow convention: 0=success, non-zero=failure
// - SQL Server specific exceptions (SqlException) should be handled appropriately
// - Transient failures should be distinguished from permanent failures
//
// Database Best Practices:
// - Always use async methods for database operations (ExecuteReaderAsync, etc.)
// - Properly dispose of SqlConnection, SqlCommand, and SqlDataReader objects
// - Use 'using' statements or 'await using' for automatic resource disposal
// - Pass CancellationToken to all database async methods
// - Use parameterized queries exclusively to prevent SQL injection
// - Implement connection pooling through connection string configuration
// - Handle SqlException with appropriate retry logic for transient errors
// - Use transactions (SqlTransaction) for operations requiring atomicity
// - Set appropriate command timeout values based on job requirements
// - Avoid holding database connections open longer than necessary
// - Use bulk operations (SqlBulkCopy) for large data transfers
// - Implement proper error logging with SQL error numbers and messages
//
// Naming Conventions:
// - Interface name: IScheduledJob (convention with 'I' prefix)
// - Method name: ExecuteAsync (Async suffix per .NET conventions)
// - Property name: JobName (PascalCase per C# conventions)
// - Namespace: AphaBatchJobsFoundation.Core.Interfaces (layered architecture)
//
// ============================================================================


**Review Summary:**

The code is well-structured and follows .NET best practices. The following refinements were made:

1. **Removed vendor-specific reference**: Changed "Apha convention" to generic "convention" in XML documentation to maintain neutrality
2. **Enhanced database guidance**: Added SQL Server-specific best practices in the remarks section of ExecuteAsync method
3. **Expanded implementation notes**: Added comprehensive "Database Best Practices" section covering:
   - Async database operations
   - Resource disposal patterns
   - Connection management
   - SQL injection prevention
   - Transaction handling
   - Error handling for SQL Server
   - Performance optimization techniques

The interface itself remains unchanged as it's already well-designed and follows .NET interface best practices. The enhancements focus on providing better guidance for implementers working with SQL Server databases.