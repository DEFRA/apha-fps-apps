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
    private readonly IExecutionYearContext _executionYearContext;
    private readonly IEmailNotificationService _notificationService;
    private readonly ILogger<MabArchiveLoadOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MabArchiveLoadOrchestrator"/> class.
    /// </summary>
    /// <param name="totalsService">Service for rebuilding FPS source totals.</param>
    /// <param name="dataService">Service for archive delete/load/refresh operations.</param>
    /// <param name="notificationService">Service used to send failure notifications.</param>
    /// <param name="logger">Logger instance.</param>
    public MabArchiveLoadOrchestrator(
        IReloadFpsTotalsService totalsService,
        IMyFpsYearlyDataService dataService,
        IExecutionYearContext executionYearContext,
        IEmailNotificationService notificationService,
        ILogger<MabArchiveLoadOrchestrator> logger)
    {
        _totalsService = totalsService ?? throw new ArgumentNullException(nameof(totalsService));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Builds the execution context from the current execution date.
    /// </summary>
    /// <returns>Computed execution context for the current run window.</returns>
    public MabArchiveExecutionContext BuildExecutionContext()
    {
        var utcNow = DateTime.UtcNow;

        // Optional test hook for deterministic local verification of month-branch behavior.
        var overrideUtcNow = Environment.GetEnvironmentVariable("MABARCHIVE_TEST_UTCNOW");
        if (!string.IsNullOrWhiteSpace(overrideUtcNow)
            && DateTime.TryParse(
                overrideUtcNow,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out var parsedUtcNow))
        {
            utcNow = parsedUtcNow;
        }

        var currentYear = utcNow.Year;
        var currentMonth = utcNow.Month;
        var previousYear = currentYear - 1;

        // Month > 4 (after April): primary year is current year.
        // Month <= 4 (April or earlier): primary year is previous year, with current-year partial refresh.
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
    /// <param name="correlationId">Correlation identifier for this execution.</param>
    /// <param name="context">Computed execution context for year/month branching.</param>
    /// <param name="transactionWrapper">Transaction wrapper delegate used to execute all work atomically.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(
        string correlationId,
        MabArchiveExecutionContext context,
        Func<Func<Task>, Task> transactionWrapper,
        CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
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
                // Legacy parity: previous year full cycle is always attempted first.
                await ExecuteFullYearCycleAsync(context.PreviousYear, cancellationToken);

                if (context.CurrentMonth > 4)
                {
                    // Legacy parity: after April, current year also runs full cycle.
                    await ExecuteFullYearCycleAsync(context.CurrentYear, cancellationToken);
                }
                else
                {
                    // Legacy parity: before May, current year runs project-all-only refresh.
                    _executionYearContext.FpsYear = context.CurrentYear;
                    _executionYearContext.YearSource = "MABArchive.PartialRefresh";

                    var currentYearAvailable = await _dataService.IsYearAvailableAsync(context.CurrentYear, cancellationToken);
                    if (!currentYearAvailable)
                    {
                        _logger.LogInformation("Skipping current-year partial refresh because year {Year} is not available", context.CurrentYear);
                    }
                    else
                    {
                        _logger.LogInformation("Executing partial refresh for current year {CurrentYear}", context.CurrentYear);
                        var refreshedRows = await _dataService.RefreshProjectAllOnlyAsync(context.CurrentYear, cancellationToken);
                        _logger.LogInformation("Refreshed project all for year {CurrentYear} | RowsRefreshed={RowsRefreshed}", context.CurrentYear, refreshedRows);
                    }
                }

                _logger.LogInformation("MABArchive orchestration completed successfully");
            });
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "MABArchive orchestration cancelled | CorrelationId={CorrelationId}", correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MABArchive orchestration failed | CorrelationId={CorrelationId}", correlationId);

            // Send failure notification
            try
            {
                await _notificationService.SendFailureNotificationAsync(
                    correlationId,
                    "MABArchive",
                    ex.Message,
                    DateTime.UtcNow,
                    CancellationToken.None);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Failed to send failure notification for CorrelationId={CorrelationId}", correlationId);
            }

            throw;
        }
    }

    private async Task ExecuteFullYearCycleAsync(int year, CancellationToken cancellationToken)
    {
        _executionYearContext.FpsYear = year;
        _executionYearContext.YearSource = "MABArchive.FullYearCycle";

        var isAvailable = await _dataService.IsYearAvailableAsync(year, cancellationToken);
        if (!isAvailable)
        {
            _logger.LogInformation("Skipping year {Year} full cycle because year is not available", year);
            return;
        }

        _logger.LogInformation("Executing full cycle for year {Year}", year);

        var totalsRows = await _totalsService.RebuildSourceTotalsAsync(year, cancellationToken);
        _logger.LogInformation("Rebuilt source totals for year {Year} | RowsInserted={RowsInserted}", year, totalsRows);

        var deletedRows = await _dataService.DeleteYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Deleted archive data for year {Year} | RowsDeleted={RowsDeleted}", year, deletedRows);

        var loadedRows = await _dataService.LoadYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Loaded archive data for year {Year} | RowsLoaded={RowsLoaded}", year, loadedRows);
    }
}
