namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Defines the contract for scheduled jobs that run on a cron schedule.
/// Scheduled jobs are triggered by the --scheduled CLI argument and execute automatically based on their schedule.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled job asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing job metadata such as correlation ID, job name, trigger type, and start time.</param>
    /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing the job execution result with status, message, and exit code.</returns>
    Task<Models.JobExecutionResult> ExecuteAsync(Models.JobExecutionContext context, CancellationToken cancellationToken);
}


// Review Comments:
// 1. The interface is well-structured and follows .NET naming conventions
// 2. XML documentation is comprehensive and clear
// 3. The method signature follows async/await best practices with CancellationToken support
// 4. Consider using fully qualified type names or adding using statements instead of Models. prefix for better readability
// 5. The interface is minimal and focused, following the Interface Segregation Principle
// 6. No changes required - the code already follows .NET 8 best practices for interface definitions
// 7. The cancellationToken parameter is correctly positioned as the last parameter
// 8. Return type uses Task<T> appropriately for async operations