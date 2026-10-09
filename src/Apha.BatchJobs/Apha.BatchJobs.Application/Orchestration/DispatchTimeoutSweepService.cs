using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Orchestration;

/// <inheritdoc cref="IDispatchTimeoutSweepService"/>
public sealed class DispatchTimeoutSweepService : IDispatchTimeoutSweepService
{
    /// <summary>
    /// Floor for job_master.timetolive. EventBridge keeps retrying a launch for up to 3600 s, so a
    /// row is never failed before a delayed but still-valid container could have claimed it.
    /// </summary>
    internal const int MinimumTimeToLiveMinutes = 75;

    /// <summary>Shown to business users (Bulk Rates, Year End and PACT screens).</summary>
    internal const string TimedOutErrorMessage =
        "This request was not picked up for processing in time and has been marked as failed. Please submit it again.";

    internal const string TimedOutDiagnosticSummary =
        "Dispatch timeout: not claimed by the batch worker within the job's time to live.";

    /// <summary>
    /// Jobs whose rows wait for the worker in a dispatched status. Explicit on purpose: Initiated
    /// is a user-held state for the approval jobs and must never be swept for them. Scheduled
    /// jobs are absent because the worker creates their row itself.
    /// </summary>
    internal static readonly IReadOnlyList<(string JobName, JobStatus PickupStatus, DispatchClock Clock)> Rules =
    [
        (BatchJobNames.BulkTestRatesUpdate, JobStatus.Approved, DispatchClock.ApprovedAt),
        (BatchJobNames.BulkStaffRatesUpdate, JobStatus.Approved, DispatchClock.ApprovedAt),
        (BatchJobNames.BulkAnimalRatesUpdate, JobStatus.Approved, DispatchClock.ApprovedAt),
        (BatchJobNames.YearEndDataSetup, JobStatus.Approved, DispatchClock.ApprovedAt),
        (BatchJobNames.YearEndCutover, JobStatus.Approved, DispatchClock.ApprovedAt),
        (BatchJobNames.RecreateSummary, JobStatus.Initiated, DispatchClock.RequestedAt)
    ];

    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<DispatchTimeoutSweepService> _logger;

    public DispatchTimeoutSweepService(IJobExecutionRepository executionRepository, ILogger<DispatchTimeoutSweepService> logger)
    {
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SweepAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (jobName, pickupStatus, clock) in Rules)
        {
            try
            {
                var failed = await _executionRepository.FailStaleDispatchedExecutionsAsync(
                    jobName, pickupStatus, clock, MinimumTimeToLiveMinutes,
                    TimedOutErrorMessage, TimedOutDiagnosticSummary, cancellationToken);

                foreach (var row in failed)
                {
                    _logger.LogError(
                        "[{ErrorType}] Dispatched job was never picked up by a worker and was marked Failed | JobName={JobName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | PickupStatus={PickupStatus} | DispatchedAtUtc={DispatchedAtUtc} | TimeToLiveMinutes={TimeToLiveMinutes}",
                        BatchExceptionMarkers.General, jobName, row.JobQueueId, row.JobExecutionId, pickupStatus, row.DispatchedAtUtc, row.TimeToLiveMinutes);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Housekeeping only: a failure here must not stop the job this container was
                // started for.
                _logger.LogError(ex,
                    "[{ErrorType}] Dispatch-timeout sweep failed for job '{JobName}' — continuing",
                    BatchExceptionMarkers.General, jobName);
            }
        }
    }
}
