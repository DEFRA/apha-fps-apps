using AphaBatchJobs.Application.Interfaces;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Models;
using AphaBatchJobs.Core.Enums;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Application.Services;

/// <summary>
/// Implementation of IJobRunnerService that orchestrates the execution of scheduled and adhoc batch jobs.
/// This service is the main entry point for job execution triggered from the host application via CLI arguments.
/// It manages job discovery, execution context creation, correlation id generation, and result aggregation.
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
    /// <param name="scheduledJobs">Collection of all registered scheduled job implementations</param>
    /// <param name="adhocJobs">Collection of all registered adhoc job implementations</param>
    /// <param name="logger">Logger instance for correlation-based logging</param>
    /// <param name="correlationIdService">Service for generating unique correlation identifiers</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null</exception>
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
    /// Creates a unique correlation id for the execution batch, iterates through all scheduled jobs,
    /// executes each with proper logging, and aggregates results to determine the final exit code.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown support in ECS Fargate</param>
    /// <returns>
    /// Exit code where 0 indicates all jobs succeeded, or the highest non-zero exit code from any failed job.
    /// If no jobs are registered, returns 0.
    /// </returns>
    public async Task<int> RunScheduledAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = _correlationIdService.NewId();
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Starting scheduled job execution batch at {StartedAt}",
            correlationId,
            startedAt);

        // Materialize the collection once to avoid multiple enumerations
        var scheduledJobsList = _scheduledJobs.ToList();

        if (scheduledJobsList.Count == 0)
        {
            _logger.LogWarning(
                "[CorrelationId: {CorrelationId}] No scheduled jobs registered. Exiting with success code.",
                correlationId);
            return 0;
        }

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Found {JobCount} scheduled job(s) to execute",
            correlationId,
            scheduledJobsList.Count);

        var highestExitCode = 0;

        foreach (var job in scheduledJobsList)
        {
            // Check cancellation before processing each job for responsive shutdown in ECS Fargate
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "[CorrelationId: {CorrelationId}] Cancellation requested. Stopping scheduled job execution.",
                    correlationId);
                // Return non-zero exit code if any job has failed, otherwise return 1 to indicate incomplete execution
                return highestExitCode > 0 ? highestExitCode : 1;
            }

            var jobName = job.GetType().Name;
            var context = new JobExecutionContext(
                JobName: jobName,
                CorrelationId: correlationId,
                TriggerType: JobType.Scheduled,
                StartedAt: DateTimeOffset.UtcNow);

            _logger.LogInformation(
                "[CorrelationId: {CorrelationId}] Executing scheduled job: {JobName}",
                correlationId,
                jobName);

            try
            {
                var result = await job.ExecuteAsync(context, cancellationToken);

                _logger.LogInformation(
                    "[CorrelationId: {CorrelationId}] Scheduled job {JobName} completed with status: {Status}, message: {Message}, exit code: {ExitCode}",
                    correlationId,
                    jobName,
                    result.Status,
                    result.Message,
                    result.ExitCode);

                // Track the highest exit code for proper error reporting to ECS Fargate
                if (result.ExitCode > highestExitCode)
                {
                    highestExitCode = result.ExitCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[CorrelationId: {CorrelationId}] Scheduled job {JobName} threw an unhandled exception: {ExceptionMessage}",
                    correlationId,
                    jobName,
                    ex.Message);

                // Use constant for error exit code and update highest if needed
                const int errorExitCode = 1;
                if (errorExitCode > highestExitCode)
                {
                    highestExitCode = errorExitCode;
                }
            }
        }

        var completedAt = DateTimeOffset.UtcNow;
        var duration = completedAt - startedAt;

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Scheduled job execution batch completed at {CompletedAt}. Duration: {Duration}. Final exit code: {ExitCode}",
            correlationId,
            completedAt,
            duration,
            highestExitCode);

        return highestExitCode;
    }

    /// <summary>
    /// Executes a specific adhoc job identified by its name.
    /// Generates a unique correlation id, locates the job by name, creates execution context,
    /// executes the job with proper logging, and returns the exit code.
    /// </summary>
    /// <param name="jobName">The unique name of the adhoc job to execute</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown support in ECS Fargate</param>
    /// <returns>Exit code from the job execution (0 for success, non-zero for failure)</returns>
    /// <exception cref="ArgumentException">Thrown when jobName is null or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when no adhoc job with the specified name is found</exception>
    public async Task<int> RunAdhocAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
        {
            throw new ArgumentException("Job name cannot be null or whitespace", nameof(jobName));
        }

        var correlationId = _correlationIdService.NewId();
        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Starting adhoc job execution for job: {JobName} at {StartedAt}",
            correlationId,
            jobName,
            startedAt);

        // Use case-insensitive comparison for better usability in CLI scenarios
        var adhocJob = _adhocJobs.FirstOrDefault(j => 
            string.Equals(j.JobName, jobName, StringComparison.OrdinalIgnoreCase));

        if (adhocJob == null)
        {
            var availableJobs = string.Join(", ", _adhocJobs.Select(j => j.JobName));
            var errorMessage = $"Adhoc job with name '{jobName}' not found. Available jobs: {availableJobs}";
            
            _logger.LogError(
                "[CorrelationId: {CorrelationId}] {ErrorMessage}",
                correlationId,
                errorMessage);

            throw new InvalidOperationException(errorMessage);
        }

        var context = new JobExecutionContext(
            JobName: adhocJob.JobName,
            CorrelationId: correlationId,
            TriggerType: JobType.Adhoc,
            StartedAt: startedAt);

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Executing adhoc job: {JobName}",
            correlationId,
            adhocJob.JobName);

        JobExecutionResult result;

        try
        {
            result = await adhocJob.ExecuteAsync(context, cancellationToken);

            _logger.LogInformation(
                "[CorrelationId: {CorrelationId}] Adhoc job {JobName} completed with status: {Status}, message: {Message}, exit code: {ExitCode}",
                correlationId,
                adhocJob.JobName,
                result.Status,
                result.Message,
                result.ExitCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[CorrelationId: {CorrelationId}] Adhoc job {JobName} threw an unhandled exception: {ExceptionMessage}",
                correlationId,
                adhocJob.JobName,
                ex.Message);

            // Create failure result for consistent error handling
            result = JobExecutionResult.Failure($"Unhandled exception: {ex.Message}", 1);
        }

        var completedAt = DateTimeOffset.UtcNow;
        var duration = completedAt - startedAt;

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Adhoc job {JobName} execution completed at {CompletedAt}. Duration: {Duration}. Exit code: {ExitCode}",
            correlationId,
            adhocJob.JobName,
            completedAt,
            duration,
            result.ExitCode);

        return result.ExitCode;
    }
}


// Key improvements made:
// 1. Simplified exit code tracking logic - removed redundant condition (result.ExitCode != 0) since comparing with > already handles this
// 2. Made errorExitCode a const for better code clarity and potential compiler optimization
// 3. Added XML documentation for ArgumentException in RunAdhocAsync method
// 4. Added clarifying comments for ECS Fargate-specific behaviors (cancellation handling, exit codes)
// 5. Improved comment for ToList() materialization to explain the performance benefit
// 6. All existing functionality preserved without adding new features