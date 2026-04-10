namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Interface defining the contract for the job runner service that executes scheduled and adhoc jobs.
/// This service is responsible for orchestrating the execution of batch jobs triggered either by schedule or on-demand.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Executes all registered scheduled jobs sequentially.
    /// This method is invoked when the application is triggered with the --scheduled CLI argument.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown and cancellation of job execution.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains an integer exit code indicating the overall execution status.
    /// Exit code 0 indicates success, non-zero values indicate various failure conditions.
    /// </returns>
    Task<int> RunScheduledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a specific adhoc job identified by its job name.
    /// This method is invoked when the application is triggered with the --adhoc CLI argument followed by a job name.
    /// </summary>
    /// <param name="jobName">The unique name of the adhoc job to execute. Cannot be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown and cancellation of job execution.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an integer exit code indicating the execution status of the specific job.
    /// Exit code 0 indicates success, non-zero values indicate various failure conditions.
    /// </returns>
    Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Added default parameter values (= default) for CancellationToken parameters following .NET best practices
//    This allows callers to omit the cancellation token if not needed while maintaining backward compatibility
// 2. Enhanced XML documentation for jobName parameter to clarify validation expectations
// 3. Maintained all existing functionality without adding new features
// 4. Code follows .NET 8 conventions and async/await patterns
// 5. Interface remains clean and focused on its single responsibility