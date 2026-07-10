using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

/// <summary>
/// Batch job handler for BulkStaffRatesUpdate.
/// Orchestrates Staff profit-centre grade annual rate changes triggered
/// by an approved request in fps.job_queue.
/// Lock lifecycle is owned exclusively by <see cref="JobOrchestrator"/>.
/// </summary>
public sealed class BulkStaffRatesUpdateJob : IBatchJob
{
    private readonly IBulkStaffRatesService _service;
    private readonly ICorrelationService _correlationService;
    private readonly ILogger<BulkStaffRatesUpdateJob> _logger;

    public string Name => BatchJobNames.BulkStaffRatesUpdate;
    public string IdempotencyStrategy => "ApprovedRowClaimWithJobLock";
    public string? ScheduleExpression => null;
    public string? ScheduleDescription => "Manual approval-triggered Staff profit-centre grade bulk rate update";
    public int? MaxExecutionSeconds => 1800;

    public BulkStaffRatesUpdateJob(
        IBulkStaffRatesService service,
        ICorrelationService correlationService,
        ILogger<BulkStaffRatesUpdateJob> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var context = BulkRatesExecutionContext.FromEnvironment(_correlationService.GetCorrelationId());

        _logger.LogInformation(
            "BulkStaffRatesUpdate handler invoked | CorrelationId={CorrelationId} | JobExecutionId={JobExecutionId} | TriggerYear={TriggerYear}",
            context.CorrelationId,
            context.JobExecutionId,
            context.TriggerYear);

        await _service.ExecuteAsync(context, cancellationToken);
    }
}
