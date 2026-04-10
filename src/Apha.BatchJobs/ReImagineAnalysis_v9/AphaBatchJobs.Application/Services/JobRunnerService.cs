using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Enums;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Application.Services;

/// <summary>
/// Service implementation for running scheduled and adhoc jobs with correlation tracking.
/// Orchestrates job execution and aggregates results into exit codes.
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
    /// <param name="scheduledJobs">Collection of scheduled jobs to execute.</param>
    /// <param name="adhocJobs">Collection of adhoc jobs available for execution.</param>
    /// <param name="logger">Logger instance for logging job execution information.</param>
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
    /// Creates a JobExecutionContext with new correlation id and JobType.Scheduled for each job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code indicating the overall execution result. Returns 0 if all jobs succeed, otherwise returns the first non-zero exit code.</returns>
    public async Task<int> RunScheduledAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = _correlationIdService.NewId();
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Starting scheduled jobs execution. CorrelationId: {CorrelationId}, StartedAt: {StartedAt}", correlationId, startedAt);

        // Materialize the collection once to avoid multiple enumerations
        var scheduledJobsList = _scheduledJobs as IList<IScheduledJob> ?? _scheduledJobs.ToList();

        if (scheduledJobsList.Count == 0)
        {
            _logger.LogWarning("No scheduled jobs registered. CorrelationId: {CorrelationId}", correlationId);
            return 0;
        }

        var overallExitCode = 0;

        foreach (var job in scheduledJobsList)
        {
            // Check for cancellation before processing each job
            cancellationToken.ThrowIfCancellationRequested();

            var jobName = job.GetType().Name;
            var context = new JobExecutionContext(
                JobName: jobName,
                CorrelationId: correlationId,
                TriggerType: JobType.Scheduled,
                StartedAt: startedAt
            );

            try
            {
                _logger.LogInformation("Executing scheduled job: {JobName}. CorrelationId: {CorrelationId}", jobName, correlationId);

                var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(
                    "Scheduled job {JobName} completed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                    jobName, result.Status, result.Message, result.ExitCode, correlationId);

                // Capture first non-zero exit code
                if (result.ExitCode != 0 && overallExitCode == 0)
                {
                    overallExitCode = result.ExitCode;
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scheduled job {JobName} was cancelled. CorrelationId: {CorrelationId}", jobName, correlationId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled job {JobName} failed with exception. CorrelationId: {CorrelationId}", jobName, correlationId);

                if (overallExitCode == 0)
                {
                    overallExitCode = 1;
                }
            }
        }

        _logger.LogInformation("Scheduled jobs execution completed. OverallExitCode: {ExitCode}, CorrelationId: {CorrelationId}", overallExitCode, correlationId);

        return overallExitCode;
    }

    /// <summary>
    /// Runs a specific adhoc job identified by its name.
    /// Creates a JobExecutionContext with new correlation id and JobType.Adhoc.
    /// </summary>
    /// <param name="jobName">The name of the adhoc job to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>Exit code from the job execution result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jobName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="jobName"/> is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the specified job is not found.</exception>
    public async Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobName);

        var correlationId = _correlationIdService.NewId();
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Starting adhoc job execution. JobName: {JobName}, CorrelationId: {CorrelationId}, StartedAt: {StartedAt}", jobName, correlationId, startedAt);

        var job = _adhocJobs.FirstOrDefault(j => j.JobName.Equals(jobName, StringComparison.OrdinalIgnoreCase));

        if (job is null)
        {
            _logger.LogError("Adhoc job not found: {JobName}. CorrelationId: {CorrelationId}", jobName, correlationId);
            throw new InvalidOperationException($"Adhoc job '{jobName}' not found.");
        }

        var context = new JobExecutionContext(
            JobName: jobName,
            CorrelationId: correlationId,
            TriggerType: JobType.Adhoc,
            StartedAt: startedAt
        );

        try
        {
            _logger.LogInformation("Executing adhoc job: {JobName}. CorrelationId: {CorrelationId}", jobName, correlationId);

            var result = await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Adhoc job {JobName} completed. Status: {Status}, Message: {Message}, ExitCode: {ExitCode}, CorrelationId: {CorrelationId}",
                jobName, result.Status, result.Message, result.ExitCode, correlationId);

            return result.ExitCode;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Adhoc job {JobName} was cancelled. CorrelationId: {CorrelationId}", jobName, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adhoc job {JobName} failed with exception. CorrelationId: {CorrelationId}", jobName, correlationId);
            throw;
        }
    }
}


// Key improvements made:
// 1. Updated ArgumentNullException checks to use .NET 8's ArgumentNullException.ThrowIfNull() for consistency
// 2. Removed redundant nameof() parameter from ArgumentException.ThrowIfNullOrWhiteSpace() as it's already provided
// 3. Added ConfigureAwait(false) to all async calls to avoid unnecessary context capturing (best practice for library/service code)
// 4. Added explicit cancellation token checks in RunScheduledAsync loop to enable early cancellation
// 5. Added explicit OperationCanceledException handling to distinguish cancellation from other exceptions
// 6. Changed null comparison from "== null" to "is null" for modern C# pattern matching
// 7. Optimized collection materialization check using "as IList<T> ?? ToList()" pattern to avoid double enumeration
// 8. Maintained all existing functionality without adding new features