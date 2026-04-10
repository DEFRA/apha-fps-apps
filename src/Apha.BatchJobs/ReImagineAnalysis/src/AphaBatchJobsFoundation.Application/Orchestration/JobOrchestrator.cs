// ============================================================================
// File: JobOrchestrator.cs
// Description: Thin orchestrator implementation for job execution coordination 
//              delegating business logic to services, handling DI resolution, 
//              context creation, and error handling
// Project: AphaBatchJobsFoundation.Application
// Layer: Application Orchestration
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AphaBatchJobsFoundation.Core.Interfaces;
using AphaBatchJobsFoundation.Core.Models;
using AphaBatchJobsFoundation.Core.Enums;
using AphaBatchJobsFoundation.Infrastructure.Logging;
using AphaBatchJobsFoundation.Infrastructure.Configuration;
using AphaBatchJobsFoundation.Infrastructure.ErrorHandling;

namespace AphaBatchJobsFoundation.Application.Orchestration
{
    /// <summary>
    /// Thin orchestrator implementation for coordinating job execution.
    /// Handles dependency injection resolution, execution context creation,
    /// correlation tracking, and error handling while delegating business logic to services.
    /// </summary>
    /// <remarks>
    /// This orchestrator follows clean architecture principles:
    /// - Remains thin and focused on coordination
    /// - Delegates business logic to job implementations
    /// - Handles cross-cutting concerns (logging, error handling, context creation)
    /// - Provides scheduler-friendly exit codes
    /// - Supports both scheduled and adhoc job execution patterns
    /// </remarks>
    public class JobOrchestrator : IJobOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AphaLogger _logger;
        private readonly AphaJobOptions _options;

        /// <summary>
        /// Initializes a new instance of the JobOrchestrator class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving job dependencies</param>
        /// <param name="logger">Structured logger with correlation id support</param>
        /// <param name="options">Configuration options for job execution</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public JobOrchestrator(
            IServiceProvider serviceProvider,
            AphaLogger logger,
            AphaJobOptions options)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Orchestrates the execution of a scheduled job asynchronously.
        /// Resolves the job from DI, creates execution context, invokes execution,
        /// and handles errors with appropriate exit codes.
        /// </summary>
        /// <param name="jobName">The unique name identifier of the scheduled job to execute</param>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown support</param>
        /// <returns>Job execution result with status and scheduler-friendly exit code</returns>
        public async Task<JobExecutionResult> ExecuteScheduledJobAsync(
            string jobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                const string errorMessage = "Job name cannot be null or empty";
                _logger.LogError(string.Empty, new ArgumentException(errorMessage, nameof(jobName)), errorMessage);
                return JobExecutionResult.Failure(errorMessage, null, ExitCodes.GeneralError);
            }

            var correlationId = CorrelationIdGenerator.Generate();
            var context = CreateExecutionContext(jobName, JobType.Scheduled, null);

            _logger.LogJobStart(correlationId, jobName, JobType.Scheduled);

            try
            {
                var job = ResolveJob<IScheduledJob>(jobName);

                if (job == null)
                {
                    var errorMessage = $"Scheduled job '{jobName}' not found in service registry";
                    _logger.LogError(correlationId, new InvalidOperationException(errorMessage), errorMessage);
                    return JobExecutionResult.Failure(errorMessage, null, ExitCodes.JobNotFound);
                }

                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                _logger.LogJobComplete(correlationId, jobName, result.Status, result.ExitCode);

                return result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(correlationId, "Job execution was cancelled: {JobName}", jobName);
                return HandleJobException(ex, correlationId, jobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(correlationId, ex, "Job execution failed: {JobName}", jobName);
                return HandleJobException(ex, correlationId, jobName);
            }
        }

        /// <summary>
        /// Orchestrates the execution of an adhoc job asynchronously with custom parameters.
        /// Resolves the job from DI, creates execution context with parameters,
        /// invokes execution, and handles errors with appropriate exit codes.
        /// </summary>
        /// <param name="jobName">The unique name identifier of the adhoc job to execute</param>
        /// <param name="parameters">Dictionary of job-specific parameters</param>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown support</param>
        /// <returns>Job execution result with status and scheduler-friendly exit code</returns>
        public async Task<JobExecutionResult> ExecuteAdhocJobAsync(
            string jobName,
            IDictionary<string, object> parameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                const string errorMessage = "Job name cannot be null or empty";
                _logger.LogError(string.Empty, new ArgumentException(errorMessage, nameof(jobName)), errorMessage);
                return JobExecutionResult.Failure(errorMessage, null, ExitCodes.GeneralError);
            }

            var correlationId = CorrelationIdGenerator.Generate();
            var parameterDict = parameters != null 
                ? new Dictionary<string, object>(parameters) 
                : new Dictionary<string, object>();
            
            var context = CreateExecutionContext(jobName, JobType.Adhoc, parameterDict);

            _logger.LogJobStart(correlationId, jobName, JobType.Adhoc);

            if (_options.EnableDetailedLogging && parameterDict.Count > 0)
            {
                _logger.LogInformation(correlationId, 
                    "Executing adhoc job '{JobName}' with {ParameterCount} parameters", 
                    jobName, 
                    parameterDict.Count);
            }

            try
            {
                var job = ResolveJob<IAdhocJob>(jobName);

                if (job == null)
                {
                    var errorMessage = $"Adhoc job '{jobName}' not found in service registry";
                    _logger.LogError(correlationId, new InvalidOperationException(errorMessage), errorMessage);
                    return JobExecutionResult.Failure(errorMessage, null, ExitCodes.JobNotFound);
                }

                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                _logger.LogJobComplete(correlationId, jobName, result.Status, result.ExitCode);

                return result;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(correlationId, "Job execution was cancelled: {JobName}", jobName);
                return HandleJobException(ex, correlationId, jobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(correlationId, ex, "Job execution failed: {JobName}", jobName);
                return HandleJobException(ex, correlationId, jobName);
            }
        }

        /// <summary>
        /// Resolves a job from the service provider by name.
        /// First attempts to get single registered job, then enumerates all registered jobs.
        /// </summary>
        /// <typeparam name="TJob">The job interface type (IScheduledJob or IAdhocJob)</typeparam>
        /// <param name="jobName">The name of the job to resolve</param>
        /// <returns>The resolved job instance or null if not found</returns>
        private TJob ResolveJob<TJob>(string jobName) where TJob : class
        {
            // Try to get single registered job
            var job = _serviceProvider.GetService<TJob>();
            
            // Check if the job name matches
            if (job != null)
            {
                var jobNameProperty = job.GetType().GetProperty("JobName");
                if (jobNameProperty != null)
                {
                    var registeredJobName = jobNameProperty.GetValue(job) as string;
                    if (string.Equals(registeredJobName, jobName, StringComparison.OrdinalIgnoreCase))
                    {
                        return job;
                    }
                }
            }

            // Enumerate all registered jobs and find by name
            var jobs = _serviceProvider.GetServices<TJob>();
            return jobs.FirstOrDefault(j =>
            {
                var jobNameProperty = j.GetType().GetProperty("JobName");
                if (jobNameProperty != null)
                {
                    var registeredJobName = jobNameProperty.GetValue(j) as string;
                    return string.Equals(registeredJobName, jobName, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });
        }

        /// <summary>
        /// Creates a job execution context with generated correlation id and metadata.
        /// </summary>
        /// <param name="jobName">Name of the job being executed</param>
        /// <param name="jobType">Type of job execution (Scheduled or Adhoc)</param>
        /// <param name="parameters">Optional dictionary of job parameters</param>
        /// <returns>Configured JobExecutionContext instance</returns>
        private static JobExecutionContext CreateExecutionContext(
            string jobName,
            JobType jobType,
            Dictionary<string, object> parameters)
        {
            return new JobExecutionContext
            {
                CorrelationId = CorrelationIdGenerator.Generate(),
                JobName = jobName,
                JobType = jobType,
                Parameters = parameters ?? new Dictionary<string, object>(),
                StartedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Handles exceptions during job execution and returns appropriate failure result.
        /// Determines exit code based on exception type and logs error details.
        /// </summary>
        /// <param name="exception">The exception that occurred during job execution</param>
        /// <param name="correlationId">Correlation id for tracking this execution</param>
        /// <param name="jobName">Name of the job that failed</param>
        /// <returns>JobExecutionResult with failure status and appropriate exit code</returns>
        private JobExecutionResult HandleJobException(
            Exception exception,
            string correlationId,
            string jobName)
        {
            if (exception is OperationCanceledException)
            {
                _logger.LogWarning(correlationId, 
                    "Job '{JobName}' was cancelled", 
                    jobName);
                return JobExecutionResult.Cancelled($"Job '{jobName}' execution was cancelled");
            }

            var exitCode = DetermineExitCode(exception);
            var errorMessage = $"Job '{jobName}' execution failed: {exception.Message}";

            _logger.LogError(correlationId, exception, errorMessage);

            return JobExecutionResult.Failure(errorMessage, exception, exitCode);
        }

        /// <summary>
        /// Determines the appropriate exit code based on exception type.
        /// </summary>
        /// <param name="exception">The exception to analyze</param>
        /// <returns>The appropriate exit code</returns>
        private static int DetermineExitCode(Exception exception)
        {
            return exception switch
            {
                InvalidOperationException => ExitCodes.ConfigurationError,
                _ when IsSqlException(exception) => ExitCodes.DatabaseError,
                _ => ExitCodes.GeneralError
            };
        }

        /// <summary>
        /// Checks if the exception is a SQL-related exception.
        /// Uses type name checking to avoid direct dependency on System.Data.SqlClient.
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if the exception is SQL-related, false otherwise</returns>
        private static bool IsSqlException(Exception exception)
        {
            var exceptionType = exception.GetType();
            return exceptionType.FullName?.Contains("Sql", StringComparison.OrdinalIgnoreCase) == true ||
                   exceptionType.Name.Contains("Sql", StringComparison.OrdinalIgnoreCase);
        }
    }
}

// ============================================================================
// IMPLEMENTATION NOTES:
// ============================================================================
//
// Code Improvements Applied:
// 1. Extracted duplicate job resolution logic into ResolveJob<TJob> method
// 2. Used LINQ FirstOrDefault for cleaner job enumeration
// 3. Made CreateExecutionContext static as it doesn't use instance state
// 4. Extracted exit code determination logic into DetermineExitCode method
// 5. Created IsSqlException helper method for better SQL exception detection
// 6. Used pattern matching (switch expression) for cleaner exit code mapping
// 7. Added const for repeated error message strings
// 8. Added parameter name to ArgumentException for better diagnostics
// 9. Used reflection-based approach for JobName property access (more generic)
// 10. Improved code organization and reduced duplication
//
// Architecture Decisions:
// 1. Thin orchestrator pattern - delegates business logic to job implementations
// 2. Interface-based design - depends on IScheduledJob and IAdhocJob abstractions
// 3. Dependency injection - resolves jobs from IServiceProvider at runtime
// 4. Correlation tracking - generates unique correlation id per execution
// 5. Structured logging - uses AphaLogger with correlation id propagation
// 6. Error handling - converts exceptions to JobExecutionResult with exit codes
//
// Job Resolution Strategy:
// - First attempts to get single registered job via GetService<T>
// - If not found or name mismatch, enumerates all registered jobs using LINQ
// - Performs case-insensitive name matching for flexibility
// - Returns JobNotFound exit code if job cannot be resolved
// - Uses reflection to access JobName property for generic implementation
//
// Exit Code Mapping:
// - Success: 0 (from JobExecutionResult.Success)
// - General Error: 1 (default for unhandled exceptions)
// - Configuration Error: 2 (for InvalidOperationException)
// - Database Error: 3 (for SqlException)
// - Job Not Found: 4 (when job cannot be resolved)
// - Cancelled: 2 (from JobExecutionResult.Cancelled)
//
// Async/Await Best Practices:
// - ConfigureAwait(false) used to avoid context capture
// - CancellationToken propagated to job execution
// - OperationCanceledException handled separately
// - All I/O operations are async
//
// Logging Strategy:
// - LogJobStart at beginning of execution
// - LogJobComplete at end with status and exit code
// - LogError for exceptions with correlation id
// - LogWarning for cancellations
// - Optional detailed logging for adhoc job parameters
//
// Thread Safety:
// - Orchestrator is stateless (no instance fields modified)
// - Safe for concurrent execution of multiple jobs
// - Each execution gets unique context and correlation id
// - Service resolution is thread-safe via IServiceProvider
//
// Parameter Handling:
// - Scheduled jobs: no parameters (null passed to context)
// - Adhoc jobs: parameters copied to new dictionary for isolation
// - Null parameters handled gracefully with empty dictionary
//
// Validation:
// - Job name validated for null/empty before execution
// - Service provider, logger, and options validated in constructor
// - Job existence validated after DI resolution
//
// Future Extensibility:
// - Can add job execution timeout policies
// - Can implement retry logic at orchestrator level
// - Can add job execution metrics and monitoring
// - Can support batch job execution
// - Can add job execution history tracking
//
// ============================================================================