using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for adhoc jobs that are triggered manually or on-demand.
    /// Adhoc jobs are executed outside of regular scheduling intervals and are typically
    /// initiated through CLI commands, API calls, or manual triggers.
    /// 
    /// Implementations of this interface should contain the business logic for jobs that:
    /// - Need to be run on-demand based on business events
    /// - Are triggered manually by operators or administrators
    /// - Execute in response to external system requests
    /// - Require immediate execution outside of scheduled intervals
    /// 
    /// All adhoc jobs must provide a unique job name identifier and implement
    /// the asynchronous execution logic with proper cancellation support.
    /// </summary>
    public interface IAdhocJob
    {
        /// <summary>
        /// Gets the unique name identifier for the adhoc job.
        /// This name is used for job identification in logs, monitoring systems,
        /// and execution tracking. The name should be descriptive and follow
        /// naming conventions (e.g., "ManualDataSync", "EmergencyReportGeneration").
        /// 
        /// The job name must be unique across all adhoc jobs in the system and should
        /// remain consistent across deployments for proper tracking and auditing.
        /// </summary>
        /// <value>A string representing the unique job name identifier.</value>
        string JobName { get; }

        /// <summary>
        /// Executes the adhoc job logic asynchronously with the provided execution context.
        /// This method contains the core business logic for the adhoc job and is invoked
        /// by the job orchestration layer when the job is triggered manually or on-demand.
        /// 
        /// Implementations should:
        /// - Use the provided JobExecutionContext for correlation tracking and parameter access
        /// - Respect the CancellationToken for graceful shutdown and timeout handling
        /// - Return a JobExecutionResult indicating success, failure, or cancellation
        /// - Handle exceptions appropriately and return failure results with error details
        /// - Log execution progress using the correlation ID from the context
        /// - Perform all I/O and database operations asynchronously
        /// 
        /// The method should be idempotent where possible and handle partial execution
        /// scenarios gracefully to support retry mechanisms.
        /// </summary>
        /// <param name="context">
        /// The execution context containing correlation ID, job name, parameters,
        /// and timing information required for job execution.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that should be monitored during execution to support
        /// graceful cancellation when the job is stopped, times out, or the application
        /// is shutting down.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation and contains the job execution result.
        /// The result includes execution status, error information (if any), execution time,
        /// and completion timestamp.
        /// </returns>
        Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
    }
}


// Changes made:
// 1. Removed specific exception documentation (ArgumentNullException and OperationCanceledException) 
//    from the interface as these are implementation details that should be documented in concrete classes.
//    Interfaces should define contracts, not implementation-specific exceptions.
// 2. Removed "Apha" branding reference from documentation to make it more generic and maintainable.
// 3. Kept all existing functionality and structure intact - no new features added.
// 4. Maintained all XML documentation for IntelliSense support and API documentation generation.
// 5. Interface remains clean and focused on the contract definition following .NET best practices.