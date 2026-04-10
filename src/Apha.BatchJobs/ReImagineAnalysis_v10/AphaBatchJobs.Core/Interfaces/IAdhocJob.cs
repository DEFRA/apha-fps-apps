namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for adhoc batch jobs that can be executed on demand.
/// Adhoc jobs are triggered manually via CLI arguments with a specific job name parameter.
/// </summary>
public interface IAdhocJob
{
    /// <summary>
    /// Gets the unique name that identifies this adhoc job.
    /// This name is used to match and execute the job when triggered via CLI.
    /// </summary>
    string JobName { get; }

    /// <summary>
    /// Executes the adhoc job asynchronously with the provided execution context.
    /// </summary>
    /// <param name="context">The execution context containing job metadata, correlation ID, and trigger information.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the job execution result with status, message, and exit code.</returns>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
}
