using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IReloadFpsTotalsService.
/// Rebuilds FPS source totals before archive load.
/// </summary>
public sealed class ReloadFpsTotalsService : IReloadFpsTotalsService
{
    private readonly BatchJobsDbContext _context;
    private readonly IExecutionYearContext _executionYearContext;
    private readonly ILogger<ReloadFpsTotalsService> _logger;
    private readonly MabArchiveSettings _settings;

    private static readonly string[] TotalsSourceViews =
    {
        "qrytotaladditionalcosts",
        "qrytotalanimalcosts",
        "qrytotalstaffcosts",
        "qrytotaltestcosts"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadFpsTotalsService"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    public ReloadFpsTotalsService(
        BatchJobsDbContext context,
        IExecutionYearContext executionYearContext,
        ILogger<ReloadFpsTotalsService> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Rebuilds FPS source totals for the specified year.
    /// </summary>
    /// <param name="year">Target FPS year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows inserted into fps.fpsyeartotals.</returns>
    public async Task<int> RebuildSourceTotalsAsync(int? year, CancellationToken cancellationToken)
    {
        var targetYear = ResolveYear(year);

        _logger.LogInformation("Rebuilding FPS source totals for year {Year}", targetYear);

        try
        {
            if (_settings.StrictYearIsolation)
            {
                await EnsureTotalsViewsAreYearScopedAsync(cancellationToken);
                _logger.LogInformation("Strict year isolation check passed for totals source views");
            }

            // Year-scoped delete to avoid cross-year data loss in multi-year databases.
            var targetYearShort = checked((short)targetYear);
            var deleteRows = await _context.FpsYearTotals
                .Where(t => t.Year == targetYearShort)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Deleted {RowCount} existing totals rows for year {Year}", deleteRows, targetYear);

            // Rebuild totals from source using legacy sp_createFPSTotals formulas and null handling.
            var insertRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.fpsyeartotals (
    parentproject,
    program,
    totaladditionalcosts,
    totalanimalcosts,
    totalstaffcosts,
    totaltestcosts,
    totalcosts,
    custincome,
    transferincome,
    totalincome,
    budget_cvl,
    requiredprofit,
    manager,
    customer,
    projectstatus,
    pvsincome,
    plancaseworkdebit,
    totalpaycosts,
    fpsyear
)
SELECT DISTINCT
    t.parentproject,
    t.program,
    CASE
        WHEN a.totaladditionalcosts IS NULL THEN 0::money
        ELSE a.totaladditionalcosts
    END AS totaladditionalcosts,
    CASE
        WHEN an.totalanimalcosts IS NULL THEN 0
        ELSE an.totalanimalcosts::numeric::double precision
    END AS totalanimalcosts,
    CASE
        WHEN s.totalstaffcosts IS NULL THEN 0
        ELSE s.totalstaffcosts::numeric::double precision
    END AS totalstaffcosts,
    CASE
        WHEN tst.totaltestcosts IS NULL THEN 0
        ELSE tst.totaltestcosts::numeric::double precision
    END AS totaltestcosts,
    (CASE
        WHEN a.totaladditionalcosts IS NULL THEN 0
        ELSE a.totaladditionalcosts::numeric::double precision
    END) +
    (CASE
        WHEN an.totalanimalcosts IS NULL THEN 0
        ELSE an.totalanimalcosts::numeric::double precision
    END) +
    (CASE
        WHEN s.totalstaffcosts IS NULL THEN 0
        ELSE s.totalstaffcosts::numeric::double precision
    END) +
    (CASE
        WHEN tst.totaltestcosts IS NULL THEN 0
        ELSE tst.totaltestcosts::numeric::double precision
    END) +
    (CASE
        WHEN t.plancaseworkdebit IS NULL THEN 0
        ELSE t.plancaseworkdebit::numeric::double precision
    END) AS totalcosts,
    t.custincome,
    t.transferincome,
    t.custincome + t.transferincome AS totalincome,
    t.budget_cvl,
    t.profit AS requiredprofit,
    t.manager,
    t.customer,
    t.projectstatus,
    CASE
        WHEN t.pvsincome IS NULL THEN 0::money
        ELSE t.pvsincome
    END AS pvsincome,
    CASE
        WHEN t.plancaseworkdebit IS NULL THEN 0::money
        ELSE t.plancaseworkdebit
    END AS plancaseworkdebit,
    CASE
        WHEN s.totalpaycosts IS NULL THEN 0
        ELSE s.totalpaycosts::numeric::double precision
    END AS totalpaycosts,
    t.fpsyear
FROM fps.tlkpproject t
LEFT JOIN fps.qrytotaladditionalcosts a
    ON t.parentproject = a.jobcode
    AND t.fpsyear = a.fpsyear
LEFT JOIN fps.qrytotalanimalcosts an
    ON t.parentproject = an.jobcode
    AND t.fpsyear = an.fpsyear
LEFT JOIN fps.qrytotalstaffcosts s
    ON t.parentproject = s.jobcode
    AND t.fpsyear = s.fpsyear
LEFT JOIN fps.qrytotaltestcosts tst
    ON t.parentproject = tst.jobcode
    AND t.fpsyear = tst.fpsyear
WHERE t.fpsyear = {targetYear}
", cancellationToken);

            _logger.LogInformation("Inserted {RowCount} rebuilt totals rows for year {Year}", insertRows, targetYear);

            return insertRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild FPS source totals for year {Year}", targetYear);
            throw;
        }
    }

    private int ResolveYear(int? explicitYear)
    {
        if (explicitYear.HasValue)
        {
            return explicitYear.Value;
        }

        if (_executionYearContext.FpsYear.HasValue)
        {
            return _executionYearContext.FpsYear.Value;
        }

        throw new InvalidOperationException("Execution year is not set in scoped context and no explicit year was provided.");
    }

    private async Task EnsureTotalsViewsAreYearScopedAsync(CancellationToken cancellationToken)
    {
        var missingViews = await _context.Database.SqlQuery<string>($@"
SELECT v.view_name AS ""Value""
FROM (VALUES
    ('qrytotaladditionalcosts'),
    ('qrytotalanimalcosts'),
    ('qrytotalstaffcosts'),
    ('qrytotaltestcosts')
) AS v(view_name)
WHERE NOT EXISTS (
    SELECT 1
    FROM information_schema.columns c
    WHERE c.table_schema = 'fps'
      AND c.table_name = v.view_name
      AND c.column_name = 'fpsyear'
)
").ToListAsync(cancellationToken);

        if (missingViews.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Strict year isolation is enabled, but these fps source views are missing fpsyear: {string.Join(", ", missingViews)}. " +
            "Update source views to expose fpsyear before running MABArchive totals rebuild.");
    }
}
