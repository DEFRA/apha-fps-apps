using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using AphaBatchJobs.Core.Models;

namespace AphaBatchJobs.Infrastructure.Logging
{
    /// <summary>
    /// Structured logging service that enriches log entries with correlation id and job context information.
    /// Provides centralized logging functionality for all batch job operations with consistent formatting
    /// and correlation tracking across distributed systems.
    /// Implements structured logging patterns for enhanced observability and troubleshooting.
    /// </summary>
    public sealed class StructuredLogger
    {
        private readonly ILogger<StructuredLogger> _logger;
        private readonly CorrelationIdMiddleware _correlationIdMiddleware;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for writing log entries.</param>
        /// <param name="correlationIdMiddleware">The middleware for retrieving correlation id from execution context.</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or correlationIdMiddleware is null.</exception>
        public StructuredLogger(
            ILogger<StructuredLogger> logger,
            CorrelationIdMiddleware correlationIdMiddleware)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationIdMiddleware = correlationIdMiddleware ?? throw new ArgumentNullException(nameof(correlationIdMiddleware));
        }

        /// <summary>
        /// Logs an informational message enriched with correlation id.
        /// </summary>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional arguments to format into the message template.</param>
        public void LogInformation(string message, params object[] args)
        {
            var correlationId = _correlationIdMiddleware.GetCorrelationId();
            
            // Use Dictionary<string, object> for better structured logging compatibility
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
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
        /// Logs a warning message enriched with correlation id.
        /// </summary>
        /// <param name="message">The warning message template to log.</param>
        /// <param name="args">Optional arguments to format into the message template.</param>
        public void LogWarning(string message, params object[] args)
        {
            var correlationId = _correlationIdMiddleware.GetCorrelationId();
            
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
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
        /// Logs an error message with exception details and correlation id.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="message">The error message template to log.</param>
        /// <param name="args">Optional arguments to format into the message template.</param>
        public void LogError(Exception exception, string message, params object[] args)
        {
            var correlationId = _correlationIdMiddleware.GetCorrelationId();
            
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
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
        /// Logs a structured job start event with job context information.
        /// Captures job name, type, parameters, and correlation id for execution tracking.
        /// </summary>
        /// <param name="context">The job execution context containing job details.</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
        public void LogJobStart(JobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var correlationId = _correlationIdMiddleware.GetCorrelationId() ?? context.CorrelationId;

            // Use Dictionary for structured logging scope to ensure proper serialization
            var scopeState = new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["JobName"] = context.JobName,
                ["JobType"] = context.JobType.ToString(),
                ["StartedAt"] = context.StartedAt
            };

            using (_logger.BeginScope(scopeState))
            {
                _logger.LogInformation(
                    "Job execution started: {JobName} (Type: {JobType}) with {ParameterCount} parameters at {StartedAt}",
                    context.JobName,
                    context.JobType,
                    context.Parameters?.Count ?? 0,
                    context.StartedAt);

                // Check for null and count in a single condition
                if (context.Parameters?.Count > 0)
                {
                    foreach (var parameter in context.Parameters)
                    {
                        _logger.LogDebug(
                            "Job parameter: {ParameterKey} = {ParameterValue}",
                            parameter.Key,
                            parameter.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Logs a structured job completion event with execution results.
        /// Captures job status, execution time, error details (if any), and correlation id.
        /// </summary>
        /// <param name="context">The job execution context containing job details.</param>
        /// <param name="result">The job execution result containing outcome and metrics.</param>
        /// <exception cref="ArgumentNullException">Thrown when context or result is null.</exception>
        public void LogJobComplete(JobExecutionContext context, JobExecutionResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var correlationId = _correlationIdMiddleware.GetCorrelationId() ?? context.CorrelationId;

            // Use Dictionary for structured logging scope
            var scopeState = new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["JobName"] = context.JobName,
                ["JobType"] = context.JobType.ToString(),
                ["Status"] = result.Status.ToString(),
                ["ExecutionTimeMs"] = result.ExecutionTimeMs,
                ["CompletedAt"] = result.CompletedAt
            };

            using (_logger.BeginScope(scopeState))
            {
                if (result.IsSuccess())
                {
                    _logger.LogInformation(
                        "Job execution completed successfully: {JobName} (Type: {JobType}) in {ExecutionTimeMs}ms at {CompletedAt}",
                        context.JobName,
                        context.JobType,
                        result.ExecutionTimeMs,
                        result.CompletedAt);
                }
                else if (result.IsFailure())
                {
                    _logger.LogError(
                        "Job execution failed: {JobName} (Type: {JobType}) after {ExecutionTimeMs}ms at {CompletedAt}. Error: {ErrorMessage}",
                        context.JobName,
                        context.JobType,
                        result.ExecutionTimeMs,
                        result.CompletedAt,
                        result.ErrorMessage);

                    if (!string.IsNullOrWhiteSpace(result.ErrorDetails))
                    {
                        _logger.LogDebug(
                            "Job execution error details: {ErrorDetails}",
                            result.ErrorDetails);
                    }
                }
                else if (result.IsCancelled())
                {
                    _logger.LogWarning(
                        "Job execution cancelled: {JobName} (Type: {JobType}) after {ExecutionTimeMs}ms at {CompletedAt}",
                        context.JobName,
                        context.JobType,
                        result.ExecutionTimeMs,
                        result.CompletedAt);
                }
                else
                {
                    _logger.LogInformation(
                        "Job execution completed with status {Status}: {JobName} (Type: {JobType}) in {ExecutionTimeMs}ms at {CompletedAt}",
                        result.Status,
                        context.JobName,
                        context.JobType,
                        result.ExecutionTimeMs,
                        result.CompletedAt);
                }
            }
        }
    }
}


// Key improvements made:
// 1. Added System.Collections.Generic using directive for Dictionary<TKey, TValue>
// 2. Replaced anonymous objects in BeginScope with Dictionary<string, object> for better structured logging compatibility
//    - Anonymous objects may not serialize properly with all logging providers (e.g., Serilog, AWS CloudWatch)
//    - Dictionary ensures consistent key-value pair serialization across different logging sinks
// 3. Simplified null check in LogJobStart: changed "context.Parameters != null && context.Parameters.Count > 0" to "context.Parameters?.Count > 0"
// 4. Maintained all existing functionality without adding new features
// 5. Preserved all XML documentation comments and exception handling
// 6. Code follows .NET naming conventions and best practices for structured logging