using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// Orchestrator for MABArchive load operations.
/// Manages year determination, transaction lifecycle, and step sequencing.
/// </summary>
public sealed class MabArchiveLoadOrchestrator
{
    private readonly IReloadFpsTotalsService _totalsService;
    private readonly IMyFpsYearlyDataService _dataService;
    private readonly IEmailNotificationService _notificationService;
    private readonly IBatchLockRepository _lockRepository;
    private readonly ILogger<MabArchiveLoadOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MabArchiveLoadOrchestrator"/> class.
    /// </summary>
    /// <param name="totalsService">Service for rebuilding FPS source totals.</param>
    /// <param name="dataService">Service for archive delete/load/refresh operations.</param>
    /// <param name="notificationService">Service used to send failure notifications.</param>
    /// <param name="lockRepository">Lock repository for execution guard operations.</param>
    /// <param name="logger">Logger instance.</param>
    public MabArchiveLoadOrchestrator(
        IReloadFpsTotalsService totalsService,
        IMyFpsYearlyDataService dataService,
        IEmailNotificationService notificationService,
        IBatchLockRepository lockRepository,
        ILogger<MabArchiveLoadOrchestrator> logger)
    {
        _totalsService = totalsService ?? throw new ArgumentNullException(nameof(totalsService));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds the execution context based on current date.
    /// </summary>
    /// <returns>Computed execution context for the current run window.</returns>
    public MabArchiveExecutionContext BuildExecutionContext()
    {
        var utcNow = DateTime.UtcNow;
        var currentYear = utcNow.Year;
        var currentMonth = utcNow.Month;
        var previousYear = currentYear - 1;

        // Primary year determination:
        // Month > 4 (after April): primaryYear = current calendar year
        // Month ≤ 4 (April or earlier): primaryYear = previous calendar year
        var primaryYear = currentMonth > 4 ? currentYear : previousYear;

        return new MabArchiveExecutionContext(
            CurrentYear: currentYear,
            PreviousYear: previousYear,
            CurrentMonth: currentMonth,
            PrimaryYear: primaryYear,
            IncludePartialRefreshYear: currentMonth <= 4);
    }

    /// <summary>
    /// Executes the MABArchive load orchestration within a single transaction.
    /// </summary>
    /// <param name="runId">Correlation run identifier for this execution.</param>
    /// <param name="context">Computed execution context for year/month branching.</param>
    /// <param name="transactionWrapper">Transaction wrapper delegate used to execute all work atomically.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(
        string runId,
        MabArchiveExecutionContext context,
        Func<Func<Task>, Task> transactionWrapper,
        CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RunId"] = runId,
            ["PrimaryYear"] = context.PrimaryYear,
            ["CurrentMonth"] = context.CurrentMonth,
            ["IncludePartialRefresh"] = context.IncludePartialRefreshYear
        });

        _logger.LogInformation(
            "MABArchive orchestration start | PrimaryYear={PrimaryYear} | CurrentMonth={CurrentMonth} | PartialRefreshIncluded={PartialRefresh}",
            context.PrimaryYear,
            context.CurrentMonth,
            context.IncludePartialRefreshYear);

        try
        {
            await transactionWrapper(async () =>
            {
                // Execute Full Load for primary year
                _logger.LogInformation("Executing Full Load for primary year {PrimaryYear}", context.PrimaryYear);

                await _totalsService.RebuildSourceTotalsAsync(context.PrimaryYear, cancellationToken);
                _logger.LogInformation("Rebuilt source totals for year {PrimaryYear}", context.PrimaryYear);

                await _dataService.DeleteYearDataAsync(context.PrimaryYear, cancellationToken);
                _logger.LogInformation("Deleted archive data for year {PrimaryYear}", context.PrimaryYear);

                await _dataService.LoadYearDataAsync(context.PrimaryYear, cancellationToken);
                _logger.LogInformation("Loaded archive data for year {PrimaryYear}", context.PrimaryYear);

                // Execute Partial Refresh for current year if applicable (month ≤ 4)
                if (context.IncludePartialRefreshYear && context.PartialRefreshYear.HasValue)
                {
                    _logger.LogInformation("Executing Partial Refresh for current year {CurrentYear}", context.PartialRefreshYear.Value);

                    await _dataService.RefreshProjectAllOnlyAsync(context.PartialRefreshYear.Value, cancellationToken);
                    _logger.LogInformation("Refreshed project all for year {CurrentYear}", context.PartialRefreshYear.Value);
                }

                _logger.LogInformation("MABArchive orchestration completed successfully");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MABArchive orchestration failed | RunId={RunId}", runId);

            // Send failure notification
            try
            {
                await _notificationService.SendFailureNotificationAsync(
                    runId,
                    "MABArchive",
                    ex.Message,
                    DateTime.UtcNow,
                    cancellationToken);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Failed to send failure notification for RunId={RunId}", runId);
            }

            throw;
        }
    }
}
