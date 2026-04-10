using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for scheduled jobs that run on a predefined schedule.
    /// Scheduled jobs are executed automatically by the job scheduler at configured intervals
    /// (e.g., daily, hourly, cron-based schedules) and implement standardized execution patterns
    /// for consistent orchestration, logging, and error handling.
    /// 
    /// Implementations of this interface should:
    /// - Be stateless and thread-safe to support concurrent execution
    /// - Use dependency injection for all external dependencies
    /// - Delegate business logic to service layer components
    /// - Handle cancellation tokens appropriately for graceful shutdown
    /// - Return comprehensive execution results for monitoring and alerting
    /// </summary>
    public interface IScheduledJob
    {
        /// <summary>
        /// Gets the unique name identifier for the scheduled job.
        /// This name is used for job registration, logging, monitoring, and scheduler configuration.
        /// The name should be descriptive, unique across all jobs, and follow naming conventions.
        /// 
        /// Examples: "DailyReportGeneration", "HourlyDataSync", "WeeklyCleanup"
        /// </summary>
        /// <value>A unique string identifier for the job.</value>
        string JobName { get; }

        /// <summary>
        /// Executes the scheduled job logic asynchronously.
        /// This method is invoked by the job scheduler at the configured schedule interval.
        /// 
        /// The implementation should:
        /// - Use the provided context for correlation tracking and parameter access
        /// - Respect the cancellation token for graceful shutdown support
        /// - Perform all I/O and database operations asynchronously
        /// - Handle exceptions internally and return appropriate failure results
        /// - Log execution progress using structured logging with correlation ID
        /// - Return a comprehensive execution result indicating success, failure, or cancellation
        /// 
        /// The method should not throw exceptions for business logic failures; instead,
        /// it should catch exceptions and return a failure result with error details.
        /// Only infrastructure-level exceptions (e.g., out of memory) should propagate.
        /// </summary>
        /// <param name="context">
        /// The execution context containing correlation ID, job name, parameters, and timing information.
        /// This context should be used throughout the job execution for consistent tracking and logging.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that signals when the job should stop execution.
        /// Implementations must check this token periodically during long-running operations
        /// and return a cancelled result when cancellation is requested.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with execution status,
        /// error information (if applicable), execution time, and completion timestamp.
        /// </returns>
        Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
    }
}


// Review Summary:
// The interface code is well-structured and follows .NET best practices. Only minor refinement made:
// 
// 1. Removed "Apha" branding from XML documentation (line 11) to make it more generic and reusable
//    Changed: "follow Apha naming conventions" -> "follow naming conventions"
// 
// The code already demonstrates:
// ✓ Proper async/await pattern with Task<T> return type
// ✓ CancellationToken support for graceful shutdown
// ✓ Comprehensive XML documentation
// ✓ Clear separation of concerns through interface design
// ✓ Proper use of readonly property (JobName)
// ✓ Context pattern for execution tracking
// ✓ Result pattern for execution outcomes
// 
// No other changes needed as the interface is clean, idiomatic, and follows .NET conventions.