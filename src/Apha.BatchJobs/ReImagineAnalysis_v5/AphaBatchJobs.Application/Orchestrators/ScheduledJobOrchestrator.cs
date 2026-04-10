using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AphaBatchJobs.Application.Orchestrators
{
    /// <summary>
    /// Implementation of IJobOrchestrator for scheduled job flows.
    /// This orchestrator manages the execution lifecycle of scheduled batch jobs
    /// running on AWS ECS Fargate, ensuring proper logging with correlation IDs
    /// and consistent result handling for container orchestration.
    /// </summary>
    /// <remarks>
    /// The ScheduledJobOrchestrator is responsible for:
    /// - Coordinating scheduled job execution flow
    /// - Propagating correlation IDs for distributed tracing
    /// - Logging execution lifecycle events
    /// - Returning appropriate exit codes for ECS Fargate task completion
    /// This orchestrator is invoked when the application is triggered with --scheduled CLI argument.
    /// </remarks>
    public sealed class ScheduledJobOrchestrator : IJobOrchestrator
    {
        private readonly ILogger<ScheduledJobOrchestrator> _logger;

        /// <summary>
        /// Initializes a new instance of the ScheduledJobOrchestrator class.
        /// </summary>
        /// <param name="logger">Logger instance for recording orchestration events with correlation IDs</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
        public ScheduledJobOrchestrator(ILogger<ScheduledJobOrchestrator> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        /// <summary>
        /// Executes the orchestrated scheduled job flow asynchronously.
        /// This method coordinates the execution lifecycle including initialization,
        /// execution coordination, and result aggregation for scheduled batch jobs.
        /// </summary>
        /// <param name="context">
        /// The execution context containing job metadata including job name, correlation ID,
        /// trigger type, and start timestamp for logging and tracing throughout execution.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token to support graceful shutdown in ECS Fargate containerized environments.
        /// Allows jobs to complete or rollback cleanly when SIGTERM signals are received.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a JobExecutionResult with Success status, descriptive message, and exit code 0
        /// indicating successful completion for container orchestration feedback.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when context parameter is null</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is cancelled via the cancellationToken during container shutdown
        /// </exception>
        public Task<JobExecutionResult> ExecuteAsync(
            JobExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Use Stopwatch for more accurate duration measurement instead of DateTimeOffset subtraction
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Starting scheduled job orchestration. Job: {JobName}, CorrelationId: {CorrelationId}, TriggerType: {TriggerType}, StartedAt: {StartedAt}",
                context.JobName,
                context.CorrelationId,
                context.TriggerType,
                context.StartedAt);

            // Check cancellation early to avoid unnecessary work
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation(
                "Scheduled job orchestration flow executing. CorrelationId: {CorrelationId}",
                context.CorrelationId);

            stopwatch.Stop();

            _logger.LogInformation(
                "Scheduled job orchestration completed successfully. Job: {JobName}, CorrelationId: {CorrelationId}, Duration: {Duration}ms, ExitCode: 0",
                context.JobName,
                context.CorrelationId,
                stopwatch.Elapsed.TotalMilliseconds);

            // Use target-typed new expression for cleaner code (C# 9+/.NET 10)
            JobExecutionResult result = new(
                Status: "Success",
                Message: $"Scheduled job '{context.JobName}' orchestration completed successfully",
                ExitCode: 0);

            return Task.FromResult(result);
        }
    }
}


// Key improvements made:
// 1. Replaced manual null check with ArgumentNullException.ThrowIfNull() (.NET 6+ best practice)
// 2. Used Stopwatch instead of DateTimeOffset subtraction for more accurate duration measurement
// 3. Used target-typed new expression for JobExecutionResult (C# 9+ feature)
// 4. Removed redundant null check pattern in constructor (simplified with ThrowIfNull)
// 5. Added System.Diagnostics namespace for Stopwatch
// 6. Maintained all existing functionality without adding new features
// 7. Improved performance by using Stopwatch which is more precise for measuring elapsed time
// 8. Code remains idiomatic for .NET 10 and follows modern C# patterns