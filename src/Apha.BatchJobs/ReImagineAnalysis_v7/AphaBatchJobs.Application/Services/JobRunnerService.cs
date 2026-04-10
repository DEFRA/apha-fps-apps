using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using AphaBatchJobs.Core.Enums;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Application.Services;

/// <summary>
/// Implementation of IJobRunnerService that orchestrates scheduled and adhoc job execution.
/// Manages the execution lifecycle of batch jobs including context creation, execution, and result aggregation.
/// </summary>
public sealed class JobRunnerService : IJobRunnerService
{
    private readonly IEnumerable<IScheduledJob> _scheduledJobs;
    private readonly IEnumerable<IAdhocJob> _adhocJobs;
    private readonly ILogger<JobRunnerService> _logger;
    private readonly ICorrelationIdService _correlationIdService;

    /// <summary>
    /// Initializes a new instance of the JobRunnerService class.
    /// </summary>
    /// <param name="scheduledJobs">Collection of all registered scheduled jobs.</param>
    /// <param name="adhocJobs">Collection of all registered adhoc jobs.</param>
    /// <param name="logger">Logger instance for tracking job execution.</param>
    /// <param name="correlationIdService">Service for generating correlation identifiers.</param>
    public JobRunnerService(
        IEnumerable<IScheduledJob> scheduledJobs,
        IEnumerable<IAdhocJob> adhocJobs,
        ILogger<JobRunnerService> logger,
        ICorrelationIdService correlationIdService)
    {
        ArgumentNullException.ThrowIfNull(scheduledJobs);
        ArgumentNullException.ThrowIfNull(adhocJobs);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(correlationIdService);

        _scheduledJobs = scheduledJobs;
        _adhocJobs = adhocJobs;
        _logger = logger;
        _correlationIdService = correlationIdService;
    }

    /// <summary>
    /// Runs all registered scheduled jobs sequentially.
    /// Creates a unique execution context for each job and aggregates results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code 0 on success, or first non-zero exit code on failure.</returns>
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

        _logger.LogInformation(
            "Found {JobCount} scheduled job(s) to execute. CorrelationId: {CorrelationId}",
            scheduledJobsList.Count,
            correlationId);

        var overallExitCode = 0;

        foreach (var job in scheduledJobsList)
        {
            // Check for cancellation before processing each job
            cancellationToken.ThrowIfCancellationRequested();

            var jobName = job.GetType().Name;
            var startedAt = DateTimeOffset.UtcNow;

            var context = new JobExecutionContext(
                JobName: jobName,
                CorrelationId: correlationId,
                TriggerType: JobType.Scheduled,
                StartedAt: startedAt);

            _logger.LogInformation(
                "Executing scheduled job: {JobName}. CorrelationId: {CorrelationId}, StartedAt: {StartedAt}",
                jobName,
                correlationId,
                startedAt);

            try
            {
                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Scheduled job {JobName} completed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    jobName,
                    result.Status,
                    result.Message,
                    result.ExitCode,
                    correlationId);

                if (result.ExitCode != 0 && overallExitCode == 0)
                {
                    overallExitCode = result.ExitCode;
                    _logger.LogWarning(
                        "Scheduled job {JobName} failed with exit code {ExitCode}. CorrelationId: {CorrelationId}",
                        jobName,
                        result.ExitCode,
                        correlationId);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Scheduled job {JobName} was cancelled. CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);
                
                if (overallExitCode == 0)
                {
                    overallExitCode = 1;
                }
                
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Scheduled job {JobName} threw an exception. CorrelationId: {CorrelationId}",
                    jobName,
                    correlationId);

                if (overallExitCode == 0)
                {
                    overallExitCode = 1;
                }
            }
        }

        _logger.LogInformation(
            "Completed scheduled jobs execution. Overall ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
            overallExitCode,
            correlationId);

        return overallExitCode;
    }

    /// <summary>
    /// Runs a specific adhoc job identified by its name.
    /// Creates an execution context and executes the matching job.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code from the job execution result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified job name is not found.</exception>
    public async Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);

        var correlationId = _correlationIdService.NewId();

        _logger.LogInformation(
            "Starting adhoc job execution. JobName: {JobName}, CorrelationId: {CorrelationId}",
            jobName,
            correlationId);

        var adhocJob = _adhocJobs.FirstOrDefault(j => 
            string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

        if (adhocJob is null)
        {
            var errorMessage = $"Adhoc job with name '{jobName}' not found.";
            _logger.LogError(
                "{ErrorMessage} CorrelationId: {CorrelationId}",
                errorMessage,
                correlationId);
            throw new InvalidOperationException(errorMessage);
        }

        var startedAt = DateTimeOffset.UtcNow;

        var context = new JobExecutionContext(
            JobName: jobName,
            CorrelationId: correlationId,
            TriggerType: JobType.Adhoc,
            StartedAt: startedAt);

        _logger.LogInformation(
            "Executing adhoc job: {JobName}. CorrelationId: {CorrelationId}, StartedAt: {StartedAt}",
            jobName,
            correlationId,
            startedAt);

        try
        {
            var result = await adhocJob.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Adhoc job {JobName} completed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                jobName,
                result.Status,
                result.Message,
                result.ExitCode,
                correlationId);

            return result.ExitCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Adhoc job {JobName} was cancelled. CorrelationId: {CorrelationId}",
                jobName,
                correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Adhoc job {JobName} threw an exception. CorrelationId: {CorrelationId}",
                jobName,
                correlationId);
            throw;
        }
    }
}


// Key improvements made:
// 1. Replaced manual null checks with ArgumentNullException.ThrowIfNull() for consistency with .NET 10 patterns
// 2. Removed redundant nameof() parameter from ArgumentException.ThrowIfNullOrWhiteSpace() as it's inferred
// 3. Added ConfigureAwait(false) to all async calls to avoid unnecessary context capture in library code
// 4. Added cancellation token check in the loop to enable early cancellation
// 5. Added explicit handling for OperationCanceledException to distinguish cancellation from other exceptions
// 6. Optimized collection materialization by checking if already a list before calling ToList()
// 7. Changed null comparison to use 'is null' pattern for consistency with modern C# idioms
// 8. Improved exception handling to properly propagate OperationCanceledException