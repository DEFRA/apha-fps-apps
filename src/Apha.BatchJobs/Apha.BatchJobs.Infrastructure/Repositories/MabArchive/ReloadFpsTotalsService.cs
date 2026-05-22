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

            var deleteRows = await _context.RsFpsYearTotals
                .Where(row => row.FpsYear == targetYear)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Deleted {RowCount} existing totals rows for year {Year}", deleteRows, targetYear);

            // Rebuild totals from source using legacy sp_createFPSTotals formulas and null handling.
            var totalsRows = await (
                from t in _context.RsTlkpProject
                where t.FpsYear == targetYear
                join a in _context.RsQryTotalAdditionalCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = a.JobCode, a.FpsYear }
                    into additionalCostsJoin
                from a in additionalCostsJoin.DefaultIfEmpty()
                join an in _context.RsQryTotalAnimalCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = an.JobCode, an.FpsYear }
                    into animalCostsJoin
                from an in animalCostsJoin.DefaultIfEmpty()
                join s in _context.RsQryTotalStaffCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = s.JobCode, s.FpsYear }
                    into staffCostsJoin
                from s in staffCostsJoin.DefaultIfEmpty()
                join tst in _context.RsQryTotalTestCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = tst.JobCode, tst.FpsYear }
                    into testCostsJoin
                from tst in testCostsJoin.DefaultIfEmpty()
                select new
                {
                    t.ParentProject,
                    t.Program,
                    TotalAdditionalCosts = a.TotalAdditionalCosts ?? 0m,
                    TotalAnimalCosts = (double?)(an.TotalAnimalCosts ?? 0m),
                    TotalStaffCosts = (double?)(s.TotalStaffCosts ?? 0m),
                    TotalTestCosts = (double?)(tst.TotalTestCosts ?? 0m),
                    TotalCosts = (double?)(a.TotalAdditionalCosts ?? 0m)
                        + (double?)(an.TotalAnimalCosts ?? 0m)
                        + (double?)(s.TotalStaffCosts ?? 0m)
                        + (double?)(tst.TotalTestCosts ?? 0m)
                        + (double?)(t.PlanCaseworkDebit ?? 0m),
                    t.CustIncome,
                    t.TransferIncome,
                    TotalIncome = t.CustIncome + t.TransferIncome,
                    t.BudgetCvl,
                    RequiredProfit = t.Profit,
                    t.Manager,
                    t.Customer,
                    t.ProjectStatus,
                    PvsIncome = t.PvsIncome ?? 0m,
                    PlanCaseworkDebit = t.PlanCaseworkDebit ?? 0m,
                    TotalPayCosts = (double?)(s.TotalPayCosts ?? 0m),
                    t.FpsYear
                })
                .Distinct()
                .Select(r => new RsFpsYearTotalsTable
                {
                    ParentProject = r.ParentProject,
                    Program = r.Program,
                    TotalAdditionalCosts = r.TotalAdditionalCosts,
                    TotalAnimalCosts = r.TotalAnimalCosts,
                    TotalStaffCosts = r.TotalStaffCosts,
                    TotalTestCosts = r.TotalTestCosts,
                    TotalCosts = r.TotalCosts,
                    CustIncome = r.CustIncome,
                    TransferIncome = r.TransferIncome,
                    TotalIncome = r.TotalIncome,
                    BudgetCvl = r.BudgetCvl,
                    RequiredProfit = r.RequiredProfit,
                    Manager = r.Manager,
                    Customer = r.Customer,
                    ProjectStatus = r.ProjectStatus,
                    PvsIncome = r.PvsIncome,
                    PlanCaseworkDebit = r.PlanCaseworkDebit,
                    TotalPayCosts = r.TotalPayCosts,
                    FpsYear = r.FpsYear
                })
                .ToListAsync(cancellationToken);

            if (totalsRows.Count == 0)
            {
                _logger.LogInformation("Inserted 0 rebuilt totals rows for year {Year}", targetYear);
                return 0;
            }

            await _context.RsFpsYearTotals.AddRangeAsync(totalsRows, cancellationToken);
            var insertRows = await _context.SaveChangesAsync(cancellationToken);

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
