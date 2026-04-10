using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Interface defining contract for adhoc batch jobs that are triggered manually via CLI.
    /// </summary>
    public interface IAdhocJob
    {
        /// <summary>
        /// Executes the adhoc job asynchronously.
        /// </summary>
        /// <param name="context">The job execution context containing correlation ID, job type, job name, parameters, and start timestamp.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the job execution.</param>
        /// <returns>A task representing the asynchronous operation that returns the job execution result.</returns>
        Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
    }
}


// Changes made:
// 1. Added 'default' parameter value to CancellationToken to follow .NET best practices
//    This allows callers to omit the cancellation token if not needed, improving API usability
// 2. The interface is well-structured with proper XML documentation
// 3. Naming conventions follow .NET standards (PascalCase for interface and method names)
// 4. Async suffix on ExecuteAsync follows Task-based Asynchronous Pattern (TAP) guidelines
// 5. No other changes needed - the interface is clean and follows SOLID principles