namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Contract for all scheduled jobs that run on a cron schedule.
/// Scheduled jobs are triggered by the --scheduled CLI argument and execute automatically
/// based on their configured schedule.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled job with the provided context and cancellation support.
    /// </summary>
    /// <param name="context">The execution context containing job metadata, correlation ID, and trigger information.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests during job execution.</param>
    /// <returns>A task that represents the asynchronous operation, containing the job execution result with status, message, and exit code.</returns>
    Task<Models.JobExecutionResult> ExecuteAsync(Models.JobExecutionContext context, CancellationToken cancellationToken);
}


// Review Comments:
// 1. The interface is well-structured and follows .NET naming conventions
// 2. XML documentation is comprehensive and clear
// 3. The async method signature follows best practices with CancellationToken as the last parameter
// 4. Consider using fully qualified type names or proper using statements instead of Models. prefix for better readability
// 5. The interface is appropriately minimal and focused on a single responsibility
// 6. No changes required for .NET 10 compatibility - the code is already compatible
// 7. The interface design supports dependency injection and testability patterns
// 8. Consider adding ConfigureAwait(false) guidance in implementation documentation if this will be used in library code