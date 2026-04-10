using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Interface for job orchestrator to coordinate execution of scheduled and adhoc jobs.
    /// </summary>
    public interface IJobOrchestrator
    {
        /// <summary>
        /// Executes all registered scheduled jobs and returns collection of results.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Collection of job execution results.</returns>
        Task<IEnumerable<JobExecutionResult>> ExecuteScheduledJobsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a specific adhoc job by name with parameters.
        /// </summary>
        /// <param name="jobName">Name of the adhoc job to execute.</param>
        /// <param name="parameters">Dictionary of parameters to pass to the job.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>Job execution result.</returns>
        Task<JobExecutionResult> ExecuteAdhocJobAsync(
            string jobName, 
            IReadOnlyDictionary<string, string>? parameters = null, 
            CancellationToken cancellationToken = default);
    }
}


// Key improvements made:
// 1. Added default value for CancellationToken parameters (= default) - .NET best practice for optional cancellation tokens
// 2. Changed Dictionary<string, string> to IReadOnlyDictionary<string, string>? - more flexible interface type that allows read-only collections and null
// 3. Made parameters nullable and optional (= null) - allows calling without parameters for jobs that don't need them
// 4. Improved formatting with multi-line parameter declaration for better readability
// 5. All changes maintain backward compatibility while following modern .NET conventions