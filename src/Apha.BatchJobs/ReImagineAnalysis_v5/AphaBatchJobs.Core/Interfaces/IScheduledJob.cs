namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Interface contract for scheduled batch jobs that run on a cron schedule.
/// Scheduled jobs are triggered via CLI argument --scheduled and execute automatically
/// based on their configured schedule when deployed to AWS ECS Fargate.
/// All scheduled job implementations must implement this interface to be discovered
/// and executed by the JobRunnerService.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled batch job asynchronously.
    /// This method is called by the JobRunnerService when the job is triggered
    /// according to its schedule. The implementation should contain all business
    /// logic for the batch operation against the PostgreSQL database.
    /// </summary>
    /// <param name="context">
    /// The execution context containing job metadata including JobName, CorrelationId,
    /// TriggerType (Scheduled), and StartedAt timestamp. This context should be used
    /// for logging and tracking purposes throughout the job execution.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to support graceful shutdown in containerized environments.
    /// Implementations should monitor this token and cancel long-running operations
    /// when cancellation is requested, particularly important for ECS Fargate task lifecycle.
    /// </param>
    /// <returns>
    /// A Task that resolves to a JobExecutionResult containing the execution status,
    /// descriptive message, and exit code. The exit code is used by the host process
    /// to determine the container exit status in AWS ECS Fargate.
    /// </returns>
    /// <remarks>
    /// Best Practices:
    /// - Always check cancellationToken.IsCancellationRequested in long-running loops
    /// - Use the context.CorrelationId in all log statements for traceability
    /// - Return appropriate exit codes: 0 for success, non-zero for failures
    /// - Handle database connection failures gracefully with proper retry logic
    /// - Ensure idempotency where possible for safe re-execution
    /// </remarks>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
}


// Changes made:
// 1. Removed redundant 'using System.Threading;' and 'using System.Threading.Tasks;' directives
//    - These are unnecessary in .NET 10 as Task and CancellationToken are commonly available
//    - Reduces namespace pollution and improves code cleanliness
// 2. Kept only the essential 'using AphaBatchJobs.Core.Models;' directive
// 3. All other aspects remain unchanged as the interface design follows best practices:
//    - Proper async/await pattern with Task<T> return type
//    - CancellationToken support for graceful shutdown (critical for ECS Fargate)
//    - Comprehensive XML documentation
//    - Clear separation of concerns