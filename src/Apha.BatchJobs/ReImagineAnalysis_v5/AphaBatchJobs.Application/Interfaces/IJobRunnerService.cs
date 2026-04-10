// AphaBatchJobs.Application/Interfaces/IJobRunnerService.cs
// Interface for the job runner service that orchestrates execution of scheduled and adhoc batch jobs.
// This service is the main entry point for triggering job execution from the host application.

namespace AphaBatchJobs.Application.Interfaces;

/// <summary>
/// Defines the contract for the job runner service responsible for executing scheduled and adhoc batch jobs.
/// </summary>
public interface IJobRunnerService
{
    /// <summary>
    /// Executes all registered scheduled jobs sequentially.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains an integer exit code where 0 indicates success and non-zero indicates failure.
    /// </returns>
    Task<int> RunScheduledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a specific adhoc job identified by its name.
    /// </summary>
    /// <param name="jobName">The unique name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to support graceful shutdown.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains an integer exit code where 0 indicates success and non-zero indicates failure.
    /// </returns>
    Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default);
}


// Changes made:
// 1. Removed explicit 'using' statements and adopted file-scoped namespace (C# 10/.NET 6+ feature)
// 2. Added default parameter value for CancellationToken (= default) to make the API more flexible
// 3. Removed unnecessary 'System.Threading' and 'System.Threading.Tasks' using statements as they are implicitly available
// 4. Maintained all existing functionality without adding new features
// 5. Kept XML documentation intact for proper API documentation
// 6. Interface remains clean and follows .NET 10 best practices for async operations