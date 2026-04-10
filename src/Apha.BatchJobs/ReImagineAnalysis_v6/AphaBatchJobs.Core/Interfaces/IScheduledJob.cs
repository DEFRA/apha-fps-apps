using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Interface defining contract for scheduled batch jobs that run on a schedule.
    /// </summary>
    public interface IScheduledJob
    {
        /// <summary>
        /// Executes the scheduled job asynchronously.
        /// </summary>
        /// <param name="context">The job execution context containing correlation ID, job type, job name, parameters, and start timestamp.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the job execution.</param>
        /// <returns>A task representing the asynchronous operation that returns the job execution result.</returns>
        Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
    }
}


// Review Comments:
// 1. The interface is well-structured and follows .NET naming conventions
// 2. XML documentation is comprehensive and properly formatted
// 3. The async method signature correctly returns Task<T> and accepts CancellationToken
// 4. The namespace follows standard .NET conventions
// 5. No changes required - the code already follows .NET best practices
// 6. The interface is minimal and focused on a single responsibility (Single Responsibility Principle)
// 7. CancellationToken parameter is correctly placed as the last parameter following .NET conventions