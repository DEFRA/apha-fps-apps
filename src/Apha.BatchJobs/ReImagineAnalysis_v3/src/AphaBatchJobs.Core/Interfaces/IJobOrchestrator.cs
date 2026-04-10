using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Interface contract for job orchestration service that coordinates job execution.
    /// Provides methods for executing both scheduled and adhoc jobs with comprehensive
    /// parameter support and cancellation handling.
    /// 
    /// This interface serves as the primary contract between the application layer and
    /// the job execution infrastructure, enabling clean separation of concerns and
    /// facilitating testing through dependency injection.
    /// 
    /// Implementations of this interface should:
    /// - Coordinate job lifecycle management
    /// - Handle job parameter validation and transformation
    /// - Manage execution context and correlation tracking
    /// - Delegate actual business logic to appropriate job services
    /// - Ensure proper error handling and result reporting
    /// - Support graceful cancellation through CancellationToken
    /// </summary>
    public interface IJobOrchestrator
    {
        /// <summary>
        /// Executes a scheduled job by name with the provided parameters.
        /// 
        /// This method is designed to be invoked by scheduling systems (e.g., AWS EventBridge, cron)
        /// and handles the complete lifecycle of scheduled job execution including:
        /// - Job resolution and validation
        /// - Parameter binding and validation
        /// - Execution context setup with correlation tracking
        /// - Delegation to appropriate job implementation
        /// - Result collection and reporting
        /// - Error handling and logging
        /// 
        /// The method supports graceful cancellation through the provided CancellationToken,
        /// allowing schedulers to terminate long-running jobs when necessary.
        /// </summary>
        /// <param name="jobName">
        /// The unique name identifier of the scheduled job to execute.
        /// This name is used to resolve the appropriate job implementation from the service registry.
        /// Job names should follow naming conventions and be case-insensitive.
        /// </param>
        /// <param name="parameters">
        /// Dictionary of job-specific parameters required for execution.
        /// Parameter keys should match the expected parameter names defined by the job implementation.
        /// Values are provided as objects to support various data types (string, int, DateTime, etc.).
        /// Can be null or empty if the job requires no parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful termination of job execution.
        /// Implementations should monitor this token and terminate execution cleanly when cancellation is requested.
        /// This enables scheduler-initiated job cancellation and timeout handling.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with comprehensive information about
        /// the job execution outcome including status, error details, execution time, and completion timestamp.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when jobName is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified job cannot be resolved or is not registered in the service registry.
        /// </exception>
        Task<JobExecutionResult> ExecuteScheduledJobAsync(
            string jobName,
            IDictionary<string, object>? parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an adhoc job by name with the provided parameters.
        /// 
        /// This method is designed to be invoked manually through CLI commands or API endpoints
        /// and handles the complete lifecycle of adhoc job execution including:
        /// - Job resolution and validation
        /// - Parameter binding and validation
        /// - Execution context setup with correlation tracking
        /// - Delegation to appropriate job implementation
        /// - Result collection and reporting
        /// - Error handling and logging
        /// 
        /// Adhoc jobs are typically used for:
        /// - Manual data corrections or migrations
        /// - On-demand report generation
        /// - Administrative tasks triggered by operators
        /// - Testing and validation of job implementations
        /// 
        /// The method supports graceful cancellation through the provided CancellationToken,
        /// allowing operators to terminate long-running jobs when necessary.
        /// </summary>
        /// <param name="jobName">
        /// The unique name identifier of the adhoc job to execute.
        /// This name is used to resolve the appropriate job implementation from the service registry.
        /// Job names should follow naming conventions and be case-insensitive.
        /// </param>
        /// <param name="parameters">
        /// Dictionary of job-specific parameters required for execution.
        /// Parameter keys should match the expected parameter names defined by the job implementation.
        /// Values are provided as objects to support various data types (string, int, DateTime, etc.).
        /// Can be null or empty if the job requires no parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful termination of job execution.
        /// Implementations should monitor this token and terminate execution cleanly when cancellation is requested.
        /// This enables operator-initiated job cancellation and timeout handling.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a <see cref="JobExecutionResult"/> with comprehensive information about
        /// the job execution outcome including status, error details, execution time, and completion timestamp.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when jobName is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified job cannot be resolved or is not registered in the service registry.
        /// </exception>
        Task<JobExecutionResult> ExecuteAdhocJobAsync(
            string jobName,
            IDictionary<string, object>? parameters,
            CancellationToken cancellationToken = default);
    }
}


// Key improvements made:
// 1. Changed Dictionary<string, object> to IDictionary<string, object>? for better flexibility and testability
//    - IDictionary is the interface, allowing for different implementations and easier mocking
//    - Made nullable (?) to explicitly indicate parameters can be null as documented
// 2. Added default value for CancellationToken (= default) following .NET best practices
//    - Makes the API more convenient to use when cancellation is not needed
//    - Aligns with standard .NET async method patterns
// 3. Removed company-specific reference "Apha" from XML documentation to make it more generic
// 4. Improved formatting consistency with proper indentation and spacing