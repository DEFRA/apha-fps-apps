using System;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundationV3.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobsFoundationV3.Host.Services
{
    /// <summary>
    /// Background service that hosts the job scheduler in the application lifecycle.
    /// Implements IHostedService to integrate with the .NET Generic Host, managing
    /// scheduler startup and shutdown operations with proper logging and error handling.
    /// </summary>
    public sealed class SchedulerHostedService : IHostedService
    {
        private readonly IJobScheduler _jobScheduler;
        private readonly ILogger<SchedulerHostedService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerHostedService"/> class.
        /// </summary>
        /// <param name="jobScheduler">The job scheduler instance to manage.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when jobScheduler or logger is null.</exception>
        public SchedulerHostedService(
            IJobScheduler jobScheduler,
            ILogger<SchedulerHostedService> logger)
        {
            _jobScheduler = jobScheduler ?? throw new ArgumentNullException(nameof(jobScheduler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// Starts the job scheduler and logs the operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous start operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting job scheduler hosted service");
                
                await _jobScheduler.StartAsync(cancellationToken).ConfigureAwait(false);
                
                _logger.LogInformation("Job scheduler hosted service started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start job scheduler hosted service");
                throw;
            }
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// Stops the job scheduler and logs the operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous stop operation.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Stopping job scheduler hosted service");
                
                await _jobScheduler.StopAsync(cancellationToken).ConfigureAwait(false);
                
                _logger.LogInformation("Job scheduler hosted service stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while stopping job scheduler hosted service");
                // Note: Consider whether rethrowing is appropriate during shutdown.
                // In some cases, swallowing the exception might be better to allow graceful shutdown.
                throw;
            }
        }
    }
}


// Key improvements made:
// 1. Added 'sealed' modifier to the class - prevents inheritance and enables potential compiler optimizations
// 2. Added ConfigureAwait(false) to all async calls - prevents deadlocks and improves performance in library code
//    by avoiding unnecessary context capturing when the synchronization context is not needed
// 3. All other aspects of the code follow .NET best practices including:
//    - Proper null checking with ArgumentNullException
//    - Structured logging with appropriate log levels
//    - Exception handling with logging before rethrowing
//    - Comprehensive XML documentation
//    - Proper async/await patterns
