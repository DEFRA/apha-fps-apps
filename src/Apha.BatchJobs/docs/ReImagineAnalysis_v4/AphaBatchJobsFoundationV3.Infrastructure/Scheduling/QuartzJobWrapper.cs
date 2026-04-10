using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using AphaBatchJobsFoundationV3.Core.Interfaces;
using AphaBatchJobsFoundationV3.Core.Enums;

namespace AphaBatchJobsFoundationV3.Infrastructure.Scheduling
{
    /// <summary>
    /// Quartz.NET IJob wrapper that executes IBatchJob implementations.
    /// Bridges Quartz scheduling with custom batch job interface, handles correlation ID injection and exit code mapping.
    /// This wrapper enables any IBatchJob implementation to be scheduled and executed by Quartz.NET scheduler.
    /// </summary>
    public class QuartzJobWrapper : IJob
    {
        /// <summary>
        /// Job data map key used to store and retrieve the batch job type name.
        /// </summary>
        public const string JobTypeKey = "JobType";

        private readonly IServiceProvider _serviceProvider;
        private readonly ICorrelationService _correlationService;
        private readonly ILogger<QuartzJobWrapper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuartzJobWrapper"/> class.
        /// Accepts IServiceProvider and ICorrelationService parameters for dependency resolution and correlation tracking.
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving batch job dependencies.</param>
        /// <param name="correlationService">Service for managing correlation IDs throughout job execution.</param>
        /// <param name="logger">Logger instance for logging job execution details.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when serviceProvider, correlationService, or logger is null.
        /// </exception>
        public QuartzJobWrapper(
            IServiceProvider serviceProvider,
            ICorrelationService correlationService,
            ILogger<QuartzJobWrapper> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implements Quartz IJob.Execute method accepting IJobExecutionContext.
        /// Generates correlation ID, resolves IBatchJob from DI using job data map, executes job, logs result with correlation ID.
        /// </summary>
        /// <param name="context">The Quartz job execution context containing job details and data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="JobExecutionException">
        /// Thrown when job type is not found in job data map, job cannot be resolved from DI, or job execution fails.
        /// </exception>
        public async Task Execute(IJobExecutionContext context)
        {
            // Validate context parameter
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Generate and set correlation ID for this job execution
            var correlationId = _correlationService.GenerateCorrelationId();
            _correlationService.SetCorrelationId(correlationId);

            var jobKey = context.JobDetail.Key;
            
            _logger.LogInformation(
                "Starting Quartz job execution. JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                jobKey,
                correlationId);

            try
            {
                // Retrieve job type from job data map
                if (!context.JobDetail.JobDataMap.TryGetValue(JobTypeKey, out var jobTypeObj) || jobTypeObj == null)
                {
                    var errorMessage = $"Job type not found in job data map for job key: {jobKey}";
                    _logger.LogError(
                        "Job type missing. JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                        jobKey,
                        correlationId);
                    throw new JobExecutionException(errorMessage);
                }

                var jobTypeName = jobTypeObj.ToString();
                _logger.LogInformation(
                    "Resolving batch job. JobType: {JobType}, JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                    jobTypeName,
                    jobKey,
                    correlationId);

                // Create a scope for resolving the batch job
                await using var scope = _serviceProvider.CreateAsyncScope();
                var batchJob = scope.ServiceProvider.GetService<IBatchJob>();

                if (batchJob == null)
                {
                    var errorMessage = $"Failed to resolve IBatchJob from DI container. JobType: {jobTypeName}, JobKey: {jobKey}";
                    _logger.LogError(
                        "Batch job resolution failed. JobType: {JobType}, JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                        jobTypeName,
                        jobKey,
                        correlationId);
                    throw new JobExecutionException(errorMessage);
                }

                _logger.LogInformation(
                    "Executing batch job. JobName: {JobName}, JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                    batchJob.JobName,
                    jobKey,
                    correlationId);

                // Execute the batch job
                var exitCode = await batchJob.ExecuteAsync(context.CancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Batch job execution completed. JobName: {JobName}, JobKey: {JobKey}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    batchJob.JobName,
                    jobKey,
                    exitCode,
                    correlationId);

                // If exit code indicates failure, throw exception to notify Quartz
                if (exitCode != ExitCode.Success)
                {
                    var errorMessage = $"Batch job completed with non-success exit code: {exitCode}";
                    _logger.LogWarning(
                        "Batch job completed with error. JobName: {JobName}, JobKey: {JobKey}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                        batchJob.JobName,
                        jobKey,
                        exitCode,
                        correlationId);
                    throw new JobExecutionException(errorMessage);
                }
            }
            catch (JobExecutionException)
            {
                // Re-throw JobExecutionException as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception during job execution. JobKey: {JobKey}, CorrelationId: {CorrelationId}",
                    jobKey,
                    correlationId);
                throw new JobExecutionException($"Unhandled exception during job execution: {ex.Message}", ex);
            }
        }
    }
}


// Key improvements made:
// 1. Added null check for context parameter at the beginning of Execute method for defensive programming
// 2. Changed 'using var scope' to 'await using var scope' with CreateAsyncScope() for proper async disposal pattern
// 3. Added ConfigureAwait(false) to the ExecuteAsync call to avoid unnecessary context capture and improve performance
// 4. These changes follow .NET best practices for async/await patterns and proper resource disposal
