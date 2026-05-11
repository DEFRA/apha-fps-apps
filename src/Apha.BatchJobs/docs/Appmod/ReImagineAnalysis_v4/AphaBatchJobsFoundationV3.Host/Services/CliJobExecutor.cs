using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobsFoundationV3.Core.Enums;
using AphaBatchJobsFoundationV3.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobsFoundationV3.Host.Services
{
    /// <summary>
    /// Service responsible for executing batch jobs via CLI trigger mode.
    /// Resolves jobs by name from the dependency injection container, executes them with correlation tracking,
    /// and returns appropriate exit codes for process monitoring.
    /// </summary>
    public sealed class CliJobExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICorrelationService _correlationService;
        private readonly ILogger<CliJobExecutor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliJobExecutor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving batch jobs from DI container.</param>
        /// <param name="correlationService">The correlation service for generating and managing correlation IDs.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public CliJobExecutor(
            IServiceProvider serviceProvider,
            ICorrelationService correlationService,
            ILogger<CliJobExecutor> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a batch job asynchronously by resolving it from the DI container using the provided job name.
        /// Generates a correlation ID for tracking, executes the job, and returns the appropriate exit code.
        /// </summary>
        /// <param name="jobName">The unique name identifier of the batch job to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
        /// containing an <see cref="ExitCode"/> indicating the execution outcome.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when jobName is null or whitespace.</exception>
        public async Task<ExitCode> ExecuteJobAsync(string jobName, CancellationToken cancellationToken)
        {
            // Best Practice: Early validation with guard clauses
            if (string.IsNullOrWhiteSpace(jobName))
            {
                _logger.LogError("Job name cannot be null or empty");
                return ExitCode.ValidationError;
            }

            var correlationId = _correlationService.GenerateCorrelationId();
            _correlationService.SetCorrelationId(correlationId);

            _logger.LogInformation(
                "Starting CLI job execution. JobName: {JobName}, CorrelationId: {CorrelationId}",
                jobName,
                correlationId);

            try
            {
                // Best Practice: Use await using for proper async disposal
                await using var scope = _serviceProvider.CreateAsyncScope();
                var jobs = scope.ServiceProvider.GetServices<IBatchJob>();

                // Best Practice: Use null-conditional operator and pattern matching for cleaner null checks
                var job = jobs.FirstOrDefault(j => j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));

                if (job is null)
                {
                    _logger.LogError(
                        "Job not found. JobName: {JobName}, CorrelationId: {CorrelationId}",
                        jobName,
                        correlationId);
                    return ExitCode.ConfigurationError;
                }

                _logger.LogInformation(
                    "Executing job. JobName: {JobName}, CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);

                // Best Practice: Pass cancellation token to async operations
                var exitCode = await job.ExecuteAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Job execution completed. JobName: {JobName}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    jobName,
                    exitCode,
                    correlationId);

                return exitCode;
            }
            catch (OperationCanceledException)
            {
                // Best Practice: Use LogWarning for expected cancellation scenarios
                _logger.LogWarning(
                    "Job execution was cancelled. JobName: {JobName}, CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);
                return ExitCode.GeneralError;
            }
            catch (Exception ex)
            {
                // Best Practice: Always include exception in LogError for proper error tracking
                _logger.LogError(
                    ex,
                    "Unhandled exception during job execution. JobName: {JobName}, CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);
                return ExitCode.UnhandledException;
            }
        }
    }
}
