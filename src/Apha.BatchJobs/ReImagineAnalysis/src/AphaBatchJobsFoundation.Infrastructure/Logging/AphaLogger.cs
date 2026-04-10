using Microsoft.Extensions.Logging;
using AphaBatchJobsFoundation.Infrastructure.Configuration;
using AphaBatchJobsFoundation.Core.Enums;

namespace AphaBatchJobsFoundation.Infrastructure.Logging
{
    /// <summary>
    /// Wrapper class for structured logging with correlation id support using ILogger 
    /// for consistent logging across the Apha BatchJobs application.
    /// Provides standardized logging methods with correlation id tracking for distributed tracing.
    /// </summary>
    public class AphaLogger
    {
        private readonly ILogger<AphaLogger> _logger;
        private readonly AphaLoggingOptions _options;

        /// <summary>
        /// Initializes a new instance of the AphaLogger class with logger and configuration options.
        /// </summary>
        /// <param name="logger">The ILogger instance for writing log entries</param>
        /// <param name="options">Configuration options for logging behavior</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or options is null</exception>
        public AphaLogger(ILogger<AphaLogger> logger, AphaLoggingOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Logs an informational message with correlation id for request tracking.
        /// </summary>
        /// <param name="correlationId">Unique identifier for tracking related log entries</param>
        /// <param name="message">The log message template</param>
        /// <param name="args">Optional arguments for the message template</param>
        public void LogInformation(string correlationId, string message, params object[] args)
        {
            if (_options.IncludeCorrelationId && !string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [nameof(correlationId)] = correlationId
                }))
                {
                    _logger.LogInformation(message, args);
                }
            }
            else
            {
                _logger.LogInformation(message, args);
            }
        }

        /// <summary>
        /// Logs a warning message with correlation id for tracking potential issues.
        /// </summary>
        /// <param name="correlationId">Unique identifier for tracking related log entries</param>
        /// <param name="message">The log message template</param>
        /// <param name="args">Optional arguments for the message template</param>
        public void LogWarning(string correlationId, string message, params object[] args)
        {
            if (_options.IncludeCorrelationId && !string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [nameof(correlationId)] = correlationId
                }))
                {
                    _logger.LogWarning(message, args);
                }
            }
            else
            {
                _logger.LogWarning(message, args);
            }
        }

        /// <summary>
        /// Logs an error message with correlation id and exception details for troubleshooting failures.
        /// </summary>
        /// <param name="correlationId">Unique identifier for tracking related log entries</param>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="message">The log message template</param>
        /// <param name="args">Optional arguments for the message template</param>
        public void LogError(string correlationId, Exception exception, string message, params object[] args)
        {
            if (_options.IncludeCorrelationId && !string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    [nameof(correlationId)] = correlationId
                }))
                {
                    _logger.LogError(exception, message, args);
                }
            }
            else
            {
                _logger.LogError(exception, message, args);
            }
        }

        /// <summary>
        /// Logs the start of a job execution with context information including job name and type.
        /// </summary>
        /// <param name="correlationId">Unique identifier for tracking this job execution</param>
        /// <param name="jobName">Name of the job being executed</param>
        /// <param name="jobType">Type of job execution (Scheduled or Adhoc)</param>
        public void LogJobStart(string correlationId, string jobName, JobType jobType)
        {
            var jobContext = new Dictionary<string, object>
            {
                [nameof(jobName)] = jobName,
                [nameof(jobType)] = jobType.ToString(),
                ["StartTime"] = DateTime.UtcNow
            };

            if (_options.IncludeCorrelationId && !string.IsNullOrWhiteSpace(correlationId))
            {
                jobContext[nameof(correlationId)] = correlationId;
            }

            using (_logger.BeginScope(jobContext))
            {
                _logger.LogInformation("Job execution started: {JobName} ({JobType})", jobName, jobType);
            }
        }

        /// <summary>
        /// Logs the completion of a job execution with result status and exit code.
        /// </summary>
        /// <param name="correlationId">Unique identifier for tracking this job execution</param>
        /// <param name="jobName">Name of the job that completed</param>
        /// <param name="status">Final execution status of the job</param>
        /// <param name="exitCode">Exit code returned by the job (0 for success, non-zero for failure)</param>
        public void LogJobComplete(string correlationId, string jobName, JobExecutionStatus status, int exitCode)
        {
            var jobContext = new Dictionary<string, object>
            {
                [nameof(jobName)] = jobName,
                [nameof(status)] = status.ToString(),
                [nameof(exitCode)] = exitCode,
                ["EndTime"] = DateTime.UtcNow
            };

            if (_options.IncludeCorrelationId && !string.IsNullOrWhiteSpace(correlationId))
            {
                jobContext[nameof(correlationId)] = correlationId;
            }

            using (_logger.BeginScope(jobContext))
            {
                if (status == JobExecutionStatus.Completed && exitCode == 0)
                {
                    _logger.LogInformation("Job execution completed successfully: {JobName} (Status: {Status}, ExitCode: {ExitCode})", 
                        jobName, status, exitCode);
                }
                else if (status == JobExecutionStatus.Failed || exitCode != 0)
                {
                    _logger.LogError("Job execution failed: {JobName} (Status: {Status}, ExitCode: {ExitCode})", 
                        jobName, status, exitCode);
                }
                else
                {
                    _logger.LogWarning("Job execution completed with status: {JobName} (Status: {Status}, ExitCode: {ExitCode})", 
                        jobName, status, exitCode);
                }
            }
        }
    }
}


// Changes made:
// 1. Replaced hardcoded string literals "CorrelationId", "JobName", "JobType", "Status", "ExitCode" 
//    with nameof() expressions for better refactoring support and compile-time safety
// 2. This prevents typos and ensures that if parameter names change, the dictionary keys will update automatically
// 3. Note: "StartTime" and "EndTime" remain as string literals since they don't correspond to parameter names
// 4. All other functionality remains unchanged as per requirements