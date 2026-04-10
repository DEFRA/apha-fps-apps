namespace AphaBatchJobs.Application.Services;

using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using AphaBatchJobs.Core.Enums;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service implementation for running batch jobs in scheduled or adhoc mode.
/// Orchestrates the execution of registered jobs and manages their lifecycle.
/// </summary>
public sealed class JobRunnerService : IJobRunnerService
{
    private readonly IEnumerable<IScheduledJob> _scheduledJobs;
    private readonly IEnumerable<IAdhocJob> _adhocJobs;
    private readonly ILogger<JobRunnerService> _logger;
    private readonly ICorrelationIdService _correlationIdService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobRunnerService"/> class.
    /// </summary>
    /// <param name="scheduledJobs">Collection of all registered scheduled jobs.</param>
    /// <param name="adhocJobs">Collection of all registered adhoc jobs.</param>
    /// <param name="logger">Logger instance for tracking job execution.</param>
    /// <param name="correlationIdService">Service for generating correlation identifiers.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
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
    /// Creates a unique execution context for each job and tracks their results.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop execution.</param>
    /// <returns>Exit code 0 on success, or first non-zero exit code on failure.</returns>
    public async Task<int> RunScheduledAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting scheduled jobs execution");

        // Materialize the collection once to avoid multiple enumerations
        var scheduledJobsList = _scheduledJobs as IList<IScheduledJob> ?? _scheduledJobs.ToList();
        
        if (scheduledJobsList.Count == 0)
        {
            _logger.LogWarning("No scheduled jobs registered");
            return 0;
        }

        _logger.LogInformation("Found {JobCount} scheduled job(s) to execute", scheduledJobsList.Count);

        var overallExitCode = 0;

        foreach (var job in scheduledJobsList)
        {
            // Check for cancellation before processing each job
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Scheduled jobs execution cancelled");
                break;
            }

            var jobName = job.GetType().Name;
            var correlationId = _correlationIdService.NewId();
            
            var context = new JobExecutionContext(
                JobName: jobName,
                CorrelationId: correlationId,
                TriggerType: JobType.Scheduled,
                StartedAt: DateTimeOffset.UtcNow
            );

            _logger.LogInformation(
                "Executing scheduled job {JobName} with correlation ID {CorrelationId}",
                jobName,
                correlationId);

            try
            {
                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Scheduled job {JobName} completed with status {Status}, message: {Message}, exit code: {ExitCode}",
                    jobName,
                    result.Status,
                    result.Message,
                    result.ExitCode);

                if (result.ExitCode != 0 && overallExitCode == 0)
                {
                    overallExitCode = result.ExitCode;
                    _logger.LogWarning(
                        "Scheduled job {JobName} failed with exit code {ExitCode}. This will be the overall exit code.",
                        jobName,
                        result.ExitCode);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Scheduled job {JobName} was cancelled with correlation ID {CorrelationId}",
                    jobName,
                    correlationId);

                if (overallExitCode == 0)
                {
                    overallExitCode = 1;
                }
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Scheduled job {JobName} threw an unhandled exception with correlation ID {CorrelationId}",
                    jobName,
                    correlationId);

                if (overallExitCode == 0)
                {
                    overallExitCode = 1;
                }
            }
        }

        _logger.LogInformation(
            "Completed scheduled jobs execution with overall exit code {ExitCode}",
            overallExitCode);

        return overallExitCode;
    }

    /// <summary>
    /// Runs a specific adhoc job by name.
    /// Locates the job by name, creates an execution context, and executes it.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to stop execution.</param>
    /// <returns>Exit code from the job execution result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified job name is not found.</exception>
    public async Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);

        _logger.LogInformation("Starting adhoc job execution for job name: {JobName}", jobName);

        // Materialize the collection once for efficient lookup
        var adhocJobsList = _adhocJobs as IList<IAdhocJob> ?? _adhocJobs.ToList();

        var adhocJob = adhocJobsList.FirstOrDefault(j => 
            string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

        if (adhocJob is null)
        {
            var availableJobs = string.Join(", ", adhocJobsList.Select(j => j.JobName));
            var errorMessage = string.IsNullOrEmpty(availableJobs)
                ? $"Adhoc job '{jobName}' not found. No adhoc jobs are registered."
                : $"Adhoc job '{jobName}' not found. Available jobs: {availableJobs}";

            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        var correlationId = _correlationIdService.NewId();
        
        var context = new JobExecutionContext(
            JobName: adhocJob.JobName,
            CorrelationId: correlationId,
            TriggerType: JobType.Adhoc,
            StartedAt: DateTimeOffset.UtcNow
        );

        _logger.LogInformation(
            "Executing adhoc job {JobName} with correlation ID {CorrelationId}",
            adhocJob.JobName,
            correlationId);

        try
        {
            var result = await adhocJob.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Adhoc job {JobName} completed with status {Status}, message: {Message}, exit code: {ExitCode}",
                adhocJob.JobName,
                result.Status,
                result.Message,
                result.ExitCode);

            return result.ExitCode;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Adhoc job {JobName} was cancelled with correlation ID {CorrelationId}",
                adhocJob.JobName,
                correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Adhoc job {JobName} threw an unhandled exception with correlation ID {CorrelationId}",
                adhocJob.JobName,
                correlationId);
            throw;
        }
    }
}
