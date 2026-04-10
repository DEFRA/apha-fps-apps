// ============================================================================
// File: SchedulerJobTrigger.cs
// Description: Scheduler integration handler for automated job triggering 
//              with environment variable support for job name resolution
// Project: AphaBatchJobsFoundation.Host
// ============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundation.Core.Interfaces;
using AphaBatchJobsFoundation.Infrastructure.Logging;
using AphaBatchJobsFoundation.Infrastructure.ErrorHandling;

namespace AphaBatchJobsFoundation.Host.Scheduler
{
    /// <summary>
    /// Handles automated job triggering from external schedulers.
    /// Reads job name from environment variable APHA_JOB_NAME and delegates execution
    /// to the job orchestrator. Returns scheduler-friendly exit codes for monitoring.
    /// </summary>
    /// <remarks>
    /// This class serves as the integration point between external schedulers (e.g., cron, Windows Task Scheduler)
    /// and the Apha BatchJobs Foundation orchestration layer. It follows the thin orchestration pattern
    /// by delegating all business logic to the IJobOrchestrator implementation.
    /// 
    /// Environment Variable Contract:
    /// - APHA_JOB_NAME: Required. Specifies the name of the job to execute.
    /// 
    /// Exit Code Contract:
    /// - 0: Successful execution
    /// - 1: General error (validation, execution failure)
    /// - 2: Configuration error (missing or invalid environment variable)
    /// - 4: Job not found
    /// 
    /// Usage Pattern:
    /// Schedulers should set the APHA_JOB_NAME environment variable before invoking the application.
    /// The application will read this variable, execute the corresponding job, and return an appropriate exit code.
    /// </remarks>
    public sealed class SchedulerJobTrigger
    {
        private readonly IJobOrchestrator _jobOrchestrator;
        private readonly AphaLogger _logger;
        private const string JobNameEnvironmentVariable = "APHA_JOB_NAME";

        /// <summary>
        /// Initializes a new instance of the SchedulerJobTrigger class.
        /// </summary>
        /// <param name="jobOrchestrator">The job orchestrator responsible for executing scheduled jobs</param>
        /// <param name="logger">The Apha logger for structured logging with correlation id support</param>
        /// <exception cref="ArgumentNullException">Thrown when jobOrchestrator or logger is null</exception>
        public SchedulerJobTrigger(IJobOrchestrator jobOrchestrator, AphaLogger logger)
        {
            _jobOrchestrator = jobOrchestrator ?? throw new ArgumentNullException(nameof(jobOrchestrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a scheduled job by reading the job name from the APHA_JOB_NAME environment variable.
        /// Invokes the job orchestrator for execution and returns a scheduler-friendly exit code.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to support graceful shutdown</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains an exit code:
        /// - 0: Success
        /// - 1: General error
        /// - 2: Configuration error (missing/invalid environment variable)
        /// - 4: Job not found
        /// </returns>
        /// <remarks>
        /// This method orchestrates the following workflow:
        /// 1. Generate correlation id for request tracking
        /// 2. Read and validate job name from environment variable
        /// 3. Log job trigger initiation
        /// 4. Invoke job orchestrator for scheduled execution
        /// 5. Log completion with exit code
        /// 6. Return scheduler-friendly exit code
        /// 
        /// Error Handling:
        /// - Missing/invalid environment variable: Returns ConfigurationError exit code
        /// - Job execution failure: Returns exit code from JobExecutionResult
        /// - Unexpected exceptions: Returns GeneralError exit code
        /// </remarks>
        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation(correlationId, "Scheduler job trigger initiated");

                var jobName = GetJobNameFromEnvironment();

                _logger.LogInformation(correlationId, "Executing scheduled job: {JobName}", jobName);

                var result = await _jobOrchestrator.ExecuteScheduledJobAsync(jobName, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(correlationId, 
                    "Scheduled job execution completed: {JobName}, Status: {Status}, ExitCode: {ExitCode}", 
                    jobName, result.Status, result.ExitCode);

                return result.ExitCode;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(correlationId, ex, 
                    "Configuration error in scheduler job trigger: {ErrorMessage}", ex.Message);
                return ExitCodes.ConfigurationError;
            }
            catch (Exception ex)
            {
                _logger.LogError(correlationId, ex, 
                    "Unexpected error in scheduler job trigger: {ErrorMessage}", ex.Message);
                return ExitCodes.GeneralError;
            }
        }

        /// <summary>
        /// Retrieves the job name from the APHA_JOB_NAME environment variable.
        /// </summary>
        /// <returns>The job name from the environment variable</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the APHA_JOB_NAME environment variable is not set
        /// </exception>
        /// <remarks>
        /// This method reads from the process environment variables.
        /// The environment variable must be set by the scheduler before invoking the application.
        /// Validation is performed inline to ensure a valid job name is returned.
        /// </remarks>
        private string GetJobNameFromEnvironment()
        {
            var jobName = Environment.GetEnvironmentVariable(JobNameEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{JobNameEnvironmentVariable}' is not set or is empty. " +
                    "Schedulers must set this variable to specify which job to execute.");
            }

            return jobName.Trim();
        }
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Code Review Changes Applied:
// 1. Added 'sealed' modifier to class declaration
//    - Prevents inheritance and enables potential compiler optimizations
//    - Class is not designed for extension
//
// 2. Removed redundant ValidateJobName method
//    - Validation logic was duplicated in GetJobNameFromEnvironment
//    - Consolidated validation into single method for better maintainability
//    - Reduces code complexity and potential for inconsistency
//
// 3. Added ConfigureAwait(false) to async call
//    - Prevents unnecessary context capture in library/service code
//    - Improves performance by avoiding synchronization context overhead
//    - Follows .NET best practices for non-UI async code
//
// Design Decisions:
// 1. Uses environment variable APHA_JOB_NAME for job name resolution
//    - Follows standard scheduler integration patterns
//    - Allows flexible job configuration without code changes
//    - Supports multiple job types with single deployment
//
// 2. Returns integer exit codes for scheduler integration
//    - 0 for success enables scheduler success detection
//    - Non-zero codes enable scheduler failure handling and alerting
//    - Follows Unix exit code conventions
//
// 3. Generates correlation id for request tracking
//    - Enables distributed tracing across log entries
//    - Facilitates troubleshooting and debugging
//    - Links scheduler trigger to job execution logs
//
// 4. Thin orchestration layer
//    - Delegates all business logic to IJobOrchestrator
//    - Focuses on scheduler integration concerns only
//    - Maintains separation of concerns
//
// Error Handling Strategy:
// - InvalidOperationException for configuration errors (missing/invalid env var)
// - Returns ConfigurationError exit code for scheduler detection
// - Catches all exceptions to prevent unhandled exceptions
// - Logs all errors with correlation id for troubleshooting
// - Returns appropriate exit codes for scheduler monitoring
//
// Logging Strategy:
// - Logs trigger initiation with correlation id
// - Logs job name and execution start
// - Logs completion with status and exit code
// - Logs errors with exception details
// - Uses structured logging for queryability
//
// Thread Safety:
// - Stateless design (no instance state)
// - Safe for concurrent invocations
// - Each execution has isolated correlation id
//
// Cancellation Support:
// - Accepts CancellationToken parameter
// - Propagates cancellation to job orchestrator
// - Enables graceful shutdown on scheduler termination
//
// Integration Points:
// - External Schedulers: Set APHA_JOB_NAME and invoke application
// - IJobOrchestrator: Delegates job execution
// - AphaLogger: Structured logging with correlation id
// - Exit Codes: Returns scheduler-friendly codes
//
// Future Extensibility:
// - Can support additional environment variables for job parameters
// - Can add retry logic if needed
// - Can support multiple job names (comma-separated) for batch execution
// - Can add pre/post execution hooks
//
// Naming Conventions:
// - Follows Apha naming patterns
// - Clear, descriptive method names
// - Consistent with existing codebase
//
// ============================================================================