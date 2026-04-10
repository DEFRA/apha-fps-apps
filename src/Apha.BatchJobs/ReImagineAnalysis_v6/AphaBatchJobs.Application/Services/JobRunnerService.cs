using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Enums;
using AphaBatchJobs.Infrastructure.ErrorHandling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AphaBatchJobs.Application.Services
{
    /// <summary>
    /// Service responsible for running scheduled and adhoc batch jobs
    /// </summary>
    public sealed class JobRunnerService : IJobRunnerService
    {
        private readonly IJobOrchestrator _jobOrchestrator;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly GlobalExceptionHandler _exceptionHandler;
        private readonly ILogger<JobRunnerService> _logger;

        public JobRunnerService(
            IJobOrchestrator jobOrchestrator,
            ICorrelationIdService correlationIdService,
            GlobalExceptionHandler exceptionHandler,
            ILogger<JobRunnerService> logger)
        {
            _jobOrchestrator = jobOrchestrator ?? throw new ArgumentNullException(nameof(jobOrchestrator));
            _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> RunScheduledJobsAsync(CancellationToken cancellationToken)
        {
            InitializeCorrelationId();
            LogJobStart("ScheduledJobs", JobType.Scheduled);

            try
            {
                var results = await _jobOrchestrator.ExecuteScheduledJobsAsync(cancellationToken).ConfigureAwait(false);
                
                // Use Count with predicate to avoid multiple enumeration
                var resultsList = results as IList<dynamic> ?? results.ToList();

                var successCount = resultsList.Count(r => r.Status == JobExecutionStatus.Completed);
                var failureCount = resultsList.Count(r => r.Status == JobExecutionStatus.Failed);

                var overallStatus = failureCount > 0 ? JobExecutionStatus.Failed : JobExecutionStatus.Completed;
                var exitCode = ExitCodeMapper.MapToExitCode(overallStatus);

                // Log with structured logging for better observability
                _logger.LogInformation(
                    "Scheduled jobs execution summary. SuccessCount: {SuccessCount}, FailureCount: {FailureCount}, TotalCount: {TotalCount}",
                    successCount,
                    failureCount,
                    resultsList.Count);

                LogJobCompletion("ScheduledJobs", overallStatus, exitCode);

                return exitCode;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Handle cancellation gracefully
                _logger.LogWarning("Scheduled jobs execution was cancelled. CorrelationId: {CorrelationId}", 
                    _correlationIdService.GetCorrelationId());
                var exitCode = ExitCodeMapper.MapToExitCode(JobExecutionStatus.Cancelled);
                LogJobCompletion("ScheduledJobs", JobExecutionStatus.Cancelled, exitCode);
                return exitCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while running scheduled jobs. CorrelationId: {CorrelationId}", 
                    _correlationIdService.GetCorrelationId());
                var exitCode = ExitCodeMapper.MapExceptionToExitCode(ex);
                LogJobCompletion("ScheduledJobs", JobExecutionStatus.Failed, exitCode);
                return exitCode;
            }
        }

        public async Task<int> RunAdhocJobAsync(string jobName, IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken)
        {
            // Use ArgumentException.ThrowIfNullOrWhiteSpace for .NET 8+ or keep existing for compatibility
            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));
            }

            InitializeCorrelationId();
            LogJobStart(jobName, JobType.Adhoc);

            try
            {
                var result = await _jobOrchestrator.ExecuteAdhocJobAsync(jobName, parameters, cancellationToken).ConfigureAwait(false);

                var exitCode = ExitCodeMapper.MapToExitCode(result.Status);

                LogJobCompletion(jobName, result.Status, exitCode);

                return exitCode;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Handle cancellation gracefully
                _logger.LogWarning("Adhoc job '{JobName}' execution was cancelled. CorrelationId: {CorrelationId}", 
                    jobName, 
                    _correlationIdService.GetCorrelationId());
                var exitCode = ExitCodeMapper.MapToExitCode(JobExecutionStatus.Cancelled);
                LogJobCompletion(jobName, JobExecutionStatus.Cancelled, exitCode);
                return exitCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while running adhoc job '{JobName}'. CorrelationId: {CorrelationId}", 
                    jobName, 
                    _correlationIdService.GetCorrelationId());
                var exitCode = ExitCodeMapper.MapExceptionToExitCode(ex);
                LogJobCompletion(jobName, JobExecutionStatus.Failed, exitCode);
                return exitCode;
            }
        }

        private void InitializeCorrelationId()
        {
            var correlationId = _correlationIdService.GenerateCorrelationId();
            _correlationIdService.SetCorrelationId(correlationId);
        }

        private void LogJobStart(string jobName, JobType jobType)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            _logger.LogInformation(
                "Job execution started. CorrelationId: {CorrelationId}, JobName: {JobName}, JobType: {JobType}, StartedAt: {StartedAt}",
                correlationId,
                jobName,
                jobType,
                DateTimeOffset.UtcNow);
        }

        private void LogJobCompletion(string jobName, JobExecutionStatus status, int exitCode)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            
            // Use appropriate log level based on status
            var logLevel = status == JobExecutionStatus.Failed ? LogLevel.Error : LogLevel.Information;
            
            _logger.Log(
                logLevel,
                "Job execution completed. CorrelationId: {CorrelationId}, JobName: {JobName}, Status: {Status}, ExitCode: {ExitCode}, CompletedAt: {CompletedAt}",
                correlationId,
                jobName,
                status,
                exitCode,
                DateTimeOffset.UtcNow);
        }
    }
}


**Key improvements made:**

1. **Sealed class**: Added `sealed` modifier to prevent inheritance and enable potential compiler optimizations
2. **ConfigureAwait(false)**: Added to async calls to avoid unnecessary context capturing in library code
3. **OperationCanceledException handling**: Added explicit handling for cancellation scenarios with proper logging
4. **Multiple enumeration prevention**: Used type checking before ToList() to avoid unnecessary materialization
5. **Structured logging enhancement**: Added summary logging for scheduled jobs with counts
6. **Log level adjustment**: Made LogJobCompletion use appropriate log level (Error for failures, Information for success)
7. **XML documentation**: Added summary comment for the class
8. **Defensive enumeration**: Improved handling of IEnumerable results to prevent multiple enumeration issues