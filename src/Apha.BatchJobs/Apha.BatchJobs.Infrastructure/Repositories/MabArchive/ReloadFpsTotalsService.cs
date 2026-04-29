using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IReloadFpsTotalsService.
/// Rebuilds FPS source totals before archive load.
/// </summary>
public sealed class ReloadFpsTotalsService : IReloadFpsTotalsService
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<ReloadFpsTotalsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadFpsTotalsService"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    public ReloadFpsTotalsService(BatchJobsDbContext context, ILogger<ReloadFpsTotalsService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Rebuilds FPS source totals for the specified year.
    /// </summary>
    /// <param name="year">Target FPS year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows inserted into fps.fpsyeartotals.</returns>
    public async Task<int> RebuildSourceTotalsAsync(int year, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rebuilding FPS source totals for year {Year}", year);

        try
        {
            // Delete existing totals for the year
            var deleteRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM fps.fpsyeartotals
WHERE fpsyear = {year}
", cancellationToken);

            _logger.LogInformation("Deleted {RowCount} existing totals rows for year {Year}", deleteRows, year);

            // Rebuild totals from source (mirrors legacy sp_createFPSTotals logic)
            // This implementation is a placeholder; full formula parity to be implemented in next phase
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
SELECT
    LEFT(t.parentproject::text, 20),
    LEFT(t.program::text, 10),
    NULL,
    NULL,
    NULL,
    NULL,
    COALESCE(t.feccost, 0::money)::double precision,
    t.custincome,
    t.transferincome,
    (t.custincome + t.transferincome),
    t.budget_cvl,
    t.profit,
    t.manager,
    LEFT(t.customer::text, 50),
    LEFT(t.projectstatus::text, 50),
    t.pvsincome,
    t.plancaseworkdebit,
    NULL,
    t.fpsyear
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
ON CONFLICT (parentproject) DO UPDATE
SET
    program = EXCLUDED.program,
    totalcosts = EXCLUDED.totalcosts,
    custincome = EXCLUDED.custincome,
    transferincome = EXCLUDED.transferincome,
    totalincome = EXCLUDED.totalincome,
    budget_cvl = EXCLUDED.budget_cvl,
    requiredprofit = EXCLUDED.requiredprofit,
    manager = EXCLUDED.manager,
    customer = EXCLUDED.customer,
    projectstatus = EXCLUDED.projectstatus,
    pvsincome = EXCLUDED.pvsincome,
    plancaseworkdebit = EXCLUDED.plancaseworkdebit,
    fpsyear = EXCLUDED.fpsyear
", cancellationToken);

            _logger.LogInformation("Inserted {RowCount} rebuilt totals rows for year {Year}", insertRows, year);

            return insertRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild FPS source totals for year {Year}", year);
            throw;
        }
    }
}
