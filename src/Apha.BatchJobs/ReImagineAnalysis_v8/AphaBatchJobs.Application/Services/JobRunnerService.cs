using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using AphaBatchJobs.Core.Enums;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Application.Services;

/// <summary>
/// Service implementation that orchestrates the execution of scheduled and adhoc jobs.
/// This service manages job execution by iterating through registered job instances,
/// creating execution contexts with correlation tracking, and aggregating results.
/// </summary>
public sealed class JobRunnerService : IJobRunnerService
{
    private readonly IEnumerable<IScheduledJob> _scheduledJobs;
    private readonly IEnumerable<IAdhocJob> _adhocJobs;
    private readonly ILogger<JobRunnerService> _logger;
    private readonly ICorrelationIdService _correlationIdService;

    /// <summary>
    /// Initializes a new instance of the JobRunnerService with required dependencies.
    /// </summary>
    /// <param name="scheduledJobs">Collection of all registered scheduled jobs to be executed.</param>
    /// <param name="adhocJobs">Collection of all registered adhoc jobs available for on-demand execution.</param>
    /// <param name="logger">Logger instance for tracking job execution and errors.</param>
    /// <param name="correlationIdService">Service for generating unique correlation identifiers for job tracking.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    public JobRunnerService(
        IEnumerable<IScheduledJob> scheduledJobs,
        IEnumerable<IAdhocJob> adhocJobs,
        ILogger<JobRunnerService> logger,
        ICorrelationIdService correlationIdService)
    {
        _scheduledJobs = scheduledJobs ?? throw new ArgumentNullException(nameof(scheduledJobs));
        _adhocJobs = adhocJobs ?? throw new ArgumentNullException(nameof(adhocJobs));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
    }

    /// <summary>
    /// Executes all registered scheduled jobs sequentially.
    /// Creates a unique execution context for each job with correlation tracking.
    /// Aggregates results and returns an appropriate exit code based on execution outcomes.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// Exit code 0 if all jobs succeed, 1 if any job fails.
    /// Returns 0 immediately if no scheduled jobs are registered.
    /// </returns>
    public async Task<int> RunScheduledAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = _correlationIdService.NewId();
        
        _logger.LogInformation(
            "Starting scheduled jobs execution. CorrelationId: {CorrelationId}",
            correlationId);

        // Materialize the collection once to avoid multiple enumerations
        var scheduledJobsList = _scheduledJobs as IList<IScheduledJob> ?? _scheduledJobs.ToList();
        
        if (scheduledJobsList.Count == 0)
        {
            _logger.LogWarning(
                "No scheduled jobs registered. CorrelationId: {CorrelationId}",
                correlationId);
            return 0;
        }

        var hasFailures = false;
        var successCount = 0;
        var failureCount = 0;

        foreach (var job in scheduledJobsList)
        {
            // Check for cancellation before processing each job
            cancellationToken.ThrowIfCancellationRequested();

            var jobName = job.GetType().Name;
            
            var context = new JobExecutionContext(
                jobName,
                correlationId,
                JobType.Scheduled,
                DateTimeOffset.UtcNow);

            _logger.LogInformation(
                "Executing scheduled job: {JobName}. CorrelationId: {CorrelationId}",
                jobName,
                correlationId);

            try
            {
                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                if (result.ExitCode == 0)
                {
                    successCount++;
                    _logger.LogInformation(
                        "Scheduled job {JobName} completed successfully. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                        jobName,
                        result.Status,
                        result.Message,
                        result.ExitCode,
                        correlationId);
                }
                else
                {
                    hasFailures = true;
                    failureCount++;
                    _logger.LogError(
                        "Scheduled job {JobName} failed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                        jobName,
                        result.Status,
                        result.Message,
                        result.ExitCode,
                        correlationId);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Log cancellation and rethrow to allow proper cleanup
                _logger.LogWarning(
                    "Scheduled job {JobName} was cancelled. CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);
                throw;
            }
            catch (Exception ex)
            {
                hasFailures = true;
                failureCount++;
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred while executing scheduled job {JobName}. CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);
            }
        }

        var finalExitCode = hasFailures ? 1 : 0;
        
        _logger.LogInformation(
            "Scheduled jobs execution completed. Total: {Total}, Success: {Success}, Failed: {Failed}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
            scheduledJobsList.Count,
            successCount,
            failureCount,
            finalExitCode,
            correlationId);

        return finalExitCode;
    }

    /// <summary>
    /// Executes a specific adhoc job identified by its job name.
    /// Creates an execution context with correlation tracking and returns the job's exit code.
    /// </summary>
    /// <param name="jobName">The unique name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// The exit code returned by the executed job.
    /// Returns 3 (InvalidOperationException mapped code) if the job is not found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when jobName is null or whitespace.</exception>
    public async Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName, nameof(jobName));

        var correlationId = _correlationIdService.NewId();
        
        _logger.LogInformation(
            "Starting adhoc job execution. JobName: {JobName}, CorrelationId: {CorrelationId}",
            jobName,
            correlationId);

        var adhocJob = _adhocJobs.FirstOrDefault(j => 
            string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

        if (adhocJob is null)
        {
            _logger.LogError(
                "Adhoc job not found: {JobName}. CorrelationId: {CorrelationId}",
                jobName,
                correlationId);
            return 3;
        }

        var context = new JobExecutionContext(
            adhocJob.JobName,
            correlationId,
            JobType.Adhoc,
            DateTimeOffset.UtcNow);

        try
        {
            _logger.LogInformation(
                "Executing adhoc job: {JobName}. CorrelationId: {CorrelationId}",
                adhocJob.JobName,
                correlationId);

            var result = await adhocJob.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            if (result.ExitCode == 0)
            {
                _logger.LogInformation(
                    "Adhoc job {JobName} completed successfully. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    adhocJob.JobName,
                    result.Status,
                    result.Message,
                    result.ExitCode,
                    correlationId);
            }
            else
            {
                _logger.LogError(
                    "Adhoc job {JobName} failed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    adhocJob.JobName,
                    result.Status,
                    result.Message,
                    result.ExitCode,
                    correlationId);
            }

            return result.ExitCode;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Log cancellation and rethrow to allow proper cleanup
            _logger.LogWarning(
                "Adhoc job {JobName} was cancelled. CorrelationId: {CorrelationId}",
                adhocJob.JobName,
                correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred while executing adhoc job {JobName}. CorrelationId: {CorrelationId}",
                adhocJob.JobName,
                correlationId);
            return 1;
        }
    }
}


// Key improvements made:
// 1. Added ConfigureAwait(false) to all async calls to avoid unnecessary context capturing in library code
// 2. Added explicit cancellation token checks in the loop to enable early termination
// 3. Added specific handling for OperationCanceledException to distinguish cancellation from other exceptions
// 4. Optimized collection materialization to avoid multiple enumerations (as IList<T> check before ToList())
// 5. Changed null comparison to use 'is null' pattern for consistency with modern C# idioms
// 6. Added cancellation logging for better observability in production environments