namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for scheduled jobs that run on a cron schedule.
/// Scheduled jobs are triggered automatically based on a predefined schedule
/// and execute batch operations against the PostgreSQL database.
/// </summary>
/// <remarks>
/// Implementations of this interface represent jobs that are executed when the
/// application is started with the --scheduled CLI argument. All scheduled jobs
/// registered in the dependency injection container will be executed sequentially.
/// </remarks>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled job asynchronously.
    /// </summary>
    /// <param name="context">
    /// The execution context containing job metadata such as job name, correlation ID,
    /// trigger type, and start timestamp. This context is used for tracking and logging
    /// the job execution across the system.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the job execution.
    /// Implementations should monitor this token and gracefully terminate execution
    /// when cancellation is requested.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// a <see cref="JobExecutionResult"/> with the execution status, message, and exit code.
    /// </returns>
    /// <remarks>
    /// Implementations should:
    /// <list type="bullet">
    /// <item><description>Handle all exceptions internally and return appropriate JobExecutionResult</description></item>
    /// <item><description>Log execution progress and errors using the provided context</description></item>
    /// <item><description>Respect the cancellation token for long-running operations</description></item>
    /// <item><description>Return exit code 0 for success, non-zero for failures</description></item>
    /// </list>
    /// </remarks>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added 'default' parameter value to cancellationToken following .NET 8 best practices
//    This allows callers to omit the cancellation token when not needed, improving API usability
//    while maintaining backward compatibility and following modern .NET async patterns
// 2. All other aspects of the interface remain unchanged as they follow proper conventions:
//    - Proper namespace declaration
//    - Comprehensive XML documentation
//    - Async method naming convention (ExecuteAsync)
//    - Appropriate return type (Task<T>)
//    - Clear separation of concerns