namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for job orchestration within the Apha batch processing system.
/// Orchestrators coordinate the execution of complex job workflows and manage job lifecycle.
/// </summary>
public interface IJobOrchestrator
{
    /// <summary>
    /// Executes the orchestrated job workflow asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing job metadata, correlation ID, trigger type, and start time.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during job execution.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="JobExecutionResult"/> with the execution status, message, and exit code.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via <paramref name="cancellationToken"/>.</exception>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}
