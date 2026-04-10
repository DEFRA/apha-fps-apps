using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using AphaBatchJobsFoundationV3.Core.Interfaces;

namespace AphaBatchJobsFoundationV3.Infrastructure.Scheduling
{
    /// <summary>
    /// Implementation of IJobScheduler using Quartz.NET for scheduled job execution.
    /// Manages job scheduling, triggering, and lifecycle with support for graceful shutdown.
    /// Provides structured logging with correlation context for all scheduler operations.
    /// </summary>
    public sealed class QuartzJobScheduler : IJobScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<QuartzJobScheduler> _logger;
        private IScheduler? _scheduler;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuartzJobScheduler"/> class.
        /// Accepts ISchedulerFactory and ILogger parameters for scheduler creation and logging.
        /// </summary>
        /// <param name="schedulerFactory">The Quartz scheduler factory used to create scheduler instances.</param>
        /// <param name="logger">The logger instance for logging scheduler operations and lifecycle events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when schedulerFactory or logger is null.
        /// </exception>
        public QuartzJobScheduler(
            ISchedulerFactory schedulerFactory,
            ILogger<QuartzJobScheduler> logger)
        {
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the job scheduler asynchronously.
        /// Initializes Quartz scheduler from factory, starts scheduler, logs startup with correlation context.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests during startup.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous start operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when scheduler is already started or initialization fails.
        /// </exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Initializing Quartz job scheduler");

                if (_scheduler is not null)
                {
                    _logger.LogWarning("Scheduler is already initialized");
                    throw new InvalidOperationException("Scheduler is already initialized");
                }

                _scheduler = await _schedulerFactory.GetScheduler(cancellationToken).ConfigureAwait(false);

                if (_scheduler is null)
                {
                    _logger.LogError("Failed to create scheduler instance from factory");
                    throw new InvalidOperationException("Failed to create scheduler instance from factory");
                }

                _logger.LogInformation(
                    "Starting Quartz scheduler. SchedulerName: {SchedulerName}, SchedulerId: {SchedulerId}",
                    _scheduler.SchedulerName,
                    _scheduler.SchedulerInstanceId);

                await _scheduler.Start(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Quartz job scheduler started successfully. SchedulerName: {SchedulerName}, SchedulerId: {SchedulerId}, IsStarted: {IsStarted}",
                    _scheduler.SchedulerName,
                    _scheduler.SchedulerInstanceId,
                    _scheduler.IsStarted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scheduler startup was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to start Quartz job scheduler");
                throw;
            }
        }

        /// <summary>
        /// Stops the job scheduler asynchronously with graceful shutdown.
        /// Gracefully shuts down Quartz scheduler with waitForJobsToComplete parameter, logs shutdown.
        /// Ensures all running jobs complete or are properly cancelled before shutdown.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests during shutdown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_scheduler is null)
                {
                    _logger.LogWarning("Scheduler is not initialized, nothing to stop");
                    return;
                }

                if (_scheduler.IsShutdown)
                {
                    _logger.LogWarning("Scheduler is already shut down");
                    return;
                }

                _logger.LogInformation(
                    "Stopping Quartz scheduler. SchedulerName: {SchedulerName}, SchedulerId: {SchedulerId}",
                    _scheduler.SchedulerName,
                    _scheduler.SchedulerInstanceId);

                await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Quartz job scheduler stopped successfully. SchedulerName: {SchedulerName}, SchedulerId: {SchedulerId}, IsShutdown: {IsShutdown}",
                    _scheduler.SchedulerName,
                    _scheduler.SchedulerInstanceId,
                    _scheduler.IsShutdown);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scheduler shutdown was cancelled, forcing shutdown");
                
                if (_scheduler is not null && !_scheduler.IsShutdown)
                {
                    await _scheduler.Shutdown(waitForJobsToComplete: false, CancellationToken.None).ConfigureAwait(false);
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while stopping Quartz job scheduler");
                throw;
            }
        }
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to the class since it's not designed for inheritance
// 2. Changed '_scheduler' field to nullable reference type (IScheduler?) for better null-safety
// 3. Replaced 'null' checks with pattern matching ('is null' and 'is not null') for modern C# idioms
// 4. All ConfigureAwait(false) calls are properly maintained for library code
// 5. Exception handling remains comprehensive with proper logging
// 6. Structured logging with named parameters is correctly implemented
// 7. Cancellation token handling follows best practices
