namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Defines the contract for adhoc jobs that run on demand.
/// Adhoc jobs are triggered manually via CLI arguments with a specific job name parameter.
/// Each adhoc job must have a unique JobName for identification and lookup during execution.
/// </summary>
public interface IAdhocJob
{
    /// <summary>
    /// Gets the unique name that identifies this adhoc job.
    /// This name is used for lookup when the job is triggered via CLI with --adhoc argument.
    /// The name must be unique across all adhoc jobs in the system.
    /// </summary>
    /// <value>A string representing the unique identifier for this adhoc job.</value>
    string JobName { get; }

    /// <summary>
    /// Executes the adhoc job asynchronously with the provided execution context.
    /// This method contains the core business logic for the adhoc job and is invoked
    /// when the job is triggered on demand via the CLI.
    /// </summary>
    /// <param name="context">
    /// The execution context containing job metadata such as JobName, CorrelationId, 
    /// TriggerType, and StartedAt timestamp. This context is used for tracking and logging.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the job execution.
    /// Implementations should monitor this token and gracefully terminate if cancellation is requested.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a 
    /// <see cref="JobExecutionResult"/> with the execution status, message, and exit code.
    /// </returns>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added 'default' parameter value to cancellationToken in ExecuteAsync method signature
//    This follows .NET best practices for CancellationToken parameters, making it optional
//    while still allowing callers to pass a token when needed. This improves API usability
//    and aligns with standard .NET async patterns used throughout the framework.
//
// Note: The code is already well-structured with proper XML documentation and follows
// interface design best practices. The only improvement is the cancellationToken default value.