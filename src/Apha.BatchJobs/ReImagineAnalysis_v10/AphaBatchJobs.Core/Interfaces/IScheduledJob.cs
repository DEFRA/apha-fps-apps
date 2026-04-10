namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for scheduled batch jobs that run on a cron schedule.
/// Scheduled jobs are triggered by the CLI with the --scheduled argument and execute automatically
/// based on their configured schedule.
/// </summary>
public interface IScheduledJob
{
    /// <summary>
    /// Executes the scheduled job asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing job metadata, correlation ID, trigger type, and start time.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the job execution.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="JobExecutionResult"/> with the execution status, message, and exit code.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}
