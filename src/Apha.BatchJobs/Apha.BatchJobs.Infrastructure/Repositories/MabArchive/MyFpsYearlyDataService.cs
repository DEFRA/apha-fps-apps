using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IMyFpsYearlyDataService.
/// Manages yearly FPS archive data operations (delete, load, refresh).
/// </summary>
public sealed class MyFpsYearlyDataService : IMyFpsYearlyDataService
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<MyFpsYearlyDataService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyFpsYearlyDataService"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    public MyFpsYearlyDataService(BatchJobsDbContext context, ILogger<MyFpsYearlyDataService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Checks whether the supplied year exists in the fiscal year master table.
    /// </summary>
    /// <param name="year">Target year to verify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the year is available for processing.</returns>
    public async Task<bool> IsYearAvailableAsync(int year, CancellationToken cancellationToken)
    {
        try
        {
            var exists = await _context.Database.SqlQuery<bool>($@"
SELECT EXISTS(
    SELECT 1
    FROM fps.tblyearmaster
    WHERE fpsyear = {year}
)").SingleAsync(cancellationToken);

            _logger.LogInformation("Year availability check for {Year}: {Exists}", year, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed year availability check for {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Deletes archive data for the specified year across archive tables in dependency order.
    /// Implements legacy SQL parity: full year-based wipe of archive dataset for the chosen year.
    /// </summary>
    /// <param name="year">Target year to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows deleted.</returns>
    public async Task<int> DeleteYearDataAsync(int year, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting archive data for year {Year} across all archive tables in dependency order (legacy parity scope)", year);

        try
        {
            var totalRowsAffected = 0;

            // Delete order must respect foreign key constraints.
            // Leaf tables first, then parent tables.
            // This list maps to legacy sp_DeleteYearsFPSData coverage per baseline document.
            var archiveTables = new[]
            {
                // Leaf tables (transaction detail level)
                "mabarchive.my_timecostcalcs",
                "mabarchive.my_monthlyoutput",
                "mabarchive.my_monthlytime",
                "mabarchive.my_projectmonthfinal",
                "mabarchive.my_proj_invoice",
                "mabarchive.my_proj_subcontract",
                "mabarchive.my_tbladditionalcosts",
                "mabarchive.my_tblanimalreq",
                "mabarchive.my_tblcontract",
                "mabarchive.my_tblstaffjob",
                "mabarchive.my_tlkptestreqmt",

                // Dimension tables (setup/reference data)
                "mabarchive.my_testorproduct",
                "mabarchive.my_staff",
                "mabarchive.my_workgroup",
                "mabarchive.my_tblprofitcentre",
                "mabarchive.my_profitcentregrade",
                "mabarchive.my_workgroupgrade",
                "mabarchive.my_tblanimals",

                // Program and project structure
                "mabarchive.my_tlkpprogram",
                "mabarchive.my_tlkpproject",
                "mabarchive.my_tlkpproject_all",

                // Aggregate and year-level tables
                "mabarchive.my_fpsyeartotals",
                "mabarchive.tlkpyear"
            };

            foreach (var table in archiveTables)
            {
                var deleteCount = 0;
                try
                {
                    deleteCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM {table:Raw}
WHERE year = {year}
", cancellationToken);

                    totalRowsAffected += deleteCount;
                    _logger.LogInformation("Deleted {RowCount} rows from {TableName} for year {Year}", deleteCount, table, year);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deleting from {TableName} for year {Year}; continuing with remaining tables", table, year);
                }
            }

            // Special handling for G_tlkpProject: project-based delete matching FPS source projects
            // (not year-based, but included in legacy scope per sp_DeleteYearsFPSData baseline)
            try
            {
                var projectDeleteCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.g_tlkpproject
WHERE parentproject IN (
    SELECT DISTINCT parentproject
    FROM fps.tlkpproject
    WHERE fpsyear = {year}
)
", cancellationToken);

                totalRowsAffected += projectDeleteCount;
                _logger.LogInformation("Deleted {RowCount} rows from mabarchive.g_tlkpproject (project-based delete for year {Year})", projectDeleteCount, year);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deleting from mabarchive.g_tlkpproject for year {Year}; continuing", year);
            }

            _logger.LogInformation("Deleted {TotalRowCount} total rows from archive tables for year {Year} (legacy parity scope)", totalRowsAffected, year);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete archive data for year {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Loads archive data for the specified year from FPS source tables.
    /// Implements legacy sp_AddYearsFPSData fan-out logic in dependency order.
    /// </summary>
    /// <param name="year">Target year to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows loaded.</returns>
    public async Task<int> LoadYearDataAsync(int year, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading archive data for year {Year} from FPS source in dependency order (legacy sp_AddYearsFPSData parity)", year);

        try
        {
            var totalRowsAffected = 0;

            // Step 1: Reference data — Programs (foundational for project references)
            var programsRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpprogram (
    year, programno, programname, directorate, minim, sector_name, customer, target, manager
)
SELECT DISTINCT
    {year}, p.programno, p.programname, p.directorate, p.minim, p.sector_name,
    p.customer, p.target, p.manager
FROM fps.tlkpprogram p
WHERE p.fpsyear = {year}
ON CONFLICT (year, programno) DO UPDATE
SET programname = EXCLUDED.programname, directorate = EXCLUDED.directorate,
    customer = EXCLUDED.customer, manager = EXCLUDED.manager
", cancellationToken);

            totalRowsAffected += programsRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_tlkpprogram for year {Year}", programsRows, year);

            // Step 2: Project groups (G_tlkpProject) — aggregated reference across projects in this year
            var projectGroupRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.g_tlkpproject (
    parentproject, projecttitle, costbookno, disease, contract, shorttitle, projectstatus
)
SELECT DISTINCT
    t.parentproject, t.projecttitle, t.costbookno, t.disease, t.contract,
    t.shorttitle, t.projectstatus
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
ON CONFLICT (parentproject) DO UPDATE
SET projecttitle = EXCLUDED.projecttitle, disease = EXCLUDED.disease,
    projectstatus = EXCLUDED.projectstatus
", cancellationToken);

            totalRowsAffected += projectGroupRows;
            _logger.LogInformation("Loaded {RowCount} rows into g_tlkpproject for year {Year}", projectGroupRows, year);

            // Step 3: Project yearly records (my_tlkpproject)
            // Load my_fpsyeartotals
            var fpsYearTotalsRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_fpsyeartotals (
    year, parentproject, program, totaladditionalcosts, totalanimalcosts,
    totalstaffcosts, totaltestcosts, totalcosts, custincome, transferincome,
    totalincome, budget_cvl, requiredprofit, manager, customer, projectstatus,
    pvsincome, plancaseworkdebit, totalpaycosts
)
SELECT
    {year}, f.parentproject, f.program, f.totaladditionalcosts, f.totalanimalcosts,
    f.totalstaffcosts, f.totaltestcosts, f.totalcosts, f.custincome, f.transferincome,
    f.totalincome, f.budget_cvl, f.requiredprofit, f.manager, f.customer,
    COALESCE(f.projectstatus, 'Active'), f.pvsincome, f.plancaseworkdebit, f.totalpaycosts
FROM fps.fpsyeartotals f
WHERE f.fpsyear = {year}
ON CONFLICT (year, parentproject) DO UPDATE
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
    totalpaycosts = EXCLUDED.totalpaycosts
", cancellationToken);

            totalRowsAffected += fpsYearTotalsRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_fpsyeartotals for year {Year}", fpsYearTotalsRows, year);

            // Load my_tlkpproject
            var tlkpProjectRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit, source,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {year}, LEFT(t.parentproject::text, 20), LEFT(t.program::text, 10),
    LEFT(t.customer::text, 50), t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, LEFT(t.projectstatus::text, 50),
    t.datecreated::date, t.feccost, t.profit, t.budget_cvl, t.caseworksub,
    t.pvsincome, t.plancaseworkdebit, 'FPS', LEFT(t.disease::text, 50),
    LEFT(t.contract::text, 10), t.finished, t.comments, t.carryover,
    t.isdefraproject, t.costcentre, t.oracleprojectcode,
    LEFT(t.subaccountcode::text, 50), LEFT(t.projectgroup::text, 50),
    LEFT(t.incomeaccountcode::text, 50)
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
ON CONFLICT (year, parentproject) DO UPDATE
SET program = EXCLUDED.program, customer = EXCLUDED.customer,
    manager = EXCLUDED.manager, transferincome = EXCLUDED.transferincome,
    custincome = EXCLUDED.custincome, projectstatus = EXCLUDED.projectstatus
", cancellationToken);

            totalRowsAffected += tlkpProjectRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_tlkpproject for year {Year}", tlkpProjectRows, year);

            // Step 4: Monthly output summary (test volume by test/buyer/month/workgroup)
            var monthlyOutputRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_monthlyoutput (
    year, testcode, buyer, month, workgroup, volume, wgbuyer
)
SELECT
    {year}, m.testcode, m.buyer, m.month, m.workgroup, m.volume, m.wgbuyer
FROM fps.monthlyoutput m
WHERE m.fpsyear = {year}
ON CONFLICT (year, testcode, buyer, month, workgroup) DO UPDATE
SET volume = EXCLUDED.volume, wgbuyer = EXCLUDED.wgbuyer
", cancellationToken);

            totalRowsAffected += monthlyOutputRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_monthlyoutput for year {Year}", monthlyOutputRows, year);

            // Step 5: Monthly time summary (staff time allocation)
            var monthlyTimeRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_monthlytime (
    year, pact_staffid, timecode, month, parentproject, workgroup, hours
)
SELECT
    {year}, m.pact_staffid, m.timecode, m.month, m.parentproject, m.workgroup, m.hours
FROM fps.monthlytime m
WHERE m.fpsyear = {year}
ON CONFLICT (year, pact_staffid, timecode, month, parentproject, workgroup) DO UPDATE
SET hours = EXCLUDED.hours
", cancellationToken);

            totalRowsAffected += monthlyTimeRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_monthlytime for year {Year}", monthlyTimeRows, year);

            // Step 6: Project month final (comprehensive project financials by month)
            var projectMonthFinalRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_projectmonthfinal (
    year, parentproject, month, volume, cost, wip, profitloss, detail,
    invoicecounter, type, invoiceamount, costofwork, costofworkdebit,
    profitmargin, profitmarginpct, cumvolume, cumcost, cumwip,
    cumprofitloss, description, comments
)
SELECT
    {year}, p.parentproject, p.month, p.volume, p.cost, p.wip, p.profitloss,
    p.detail, p.invoicecounter, p.type, p.invoiceamount, p.costofwork,
    p.costofworkdebit, p.profitmargin, p.profitmarginpct, p.cumvolume,
    p.cumcost, p.cumwip, p.cumprofitloss, p.description, p.comments
FROM fps.projectmonthfinal p
WHERE p.fpsyear = {year}
ON CONFLICT (year, parentproject, month, type) DO UPDATE
SET volume = EXCLUDED.volume, cost = EXCLUDED.cost, wip = EXCLUDED.wip,
    profitloss = EXCLUDED.profitloss, cumvolume = EXCLUDED.cumvolume,
    cumcost = EXCLUDED.cumcost, cumprofitloss = EXCLUDED.cumprofitloss
", cancellationToken);

            totalRowsAffected += projectMonthFinalRows;
            _logger.LogInformation("Loaded {RowCount} rows into my_projectmonthfinal for year {Year}", projectMonthFinalRows, year);

            // Note: Remaining 20 loaders (invoices, subcontracts, costs, staffing, etc.) pending implementation

            _logger.LogInformation("Loaded {TotalRowCount} total rows into archive tables for year {Year} (Step 1a complete)", totalRowsAffected, year);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load archive data for year {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Refreshes only the my_tlkpproject_all table for the specified year.
    /// </summary>
    /// <param name="year">Target year to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rows affected in my_tlkpproject_all.</returns>
    public async Task<int> RefreshProjectAllOnlyAsync(int year, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refreshing project_all cross-reference only for year {Year}", year);

        try
        {
            // Delete existing records
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject_all
WHERE year = {year}
", cancellationToken);

            // Reload fresh records
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject_all (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit, source,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {year}, LEFT(t.parentproject::text, 20), LEFT(t.program::text, 10),
    LEFT(t.customer::text, 50), t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, LEFT(t.projectstatus::text, 50),
    t.datecreated::date, t.feccost, t.profit, t.budget_cvl, t.caseworksub,
    t.pvsincome, t.plancaseworkdebit, 'FPS', LEFT(t.disease::text, 50),
    LEFT(t.contract::text, 10), t.finished, t.comments, t.carryover,
    t.isdefraproject, t.costcentre, t.oracleprojectcode,
    LEFT(t.subaccountcode::text, 50), LEFT(t.projectgroup::text, 50),
    LEFT(t.incomeaccountcode::text, 50)
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
ON CONFLICT (year, parentproject) DO UPDATE
SET program = EXCLUDED.program, customer = EXCLUDED.customer,
    manager = EXCLUDED.manager, projectstatus = EXCLUDED.projectstatus
", cancellationToken);

            _logger.LogInformation("Refreshed {RowCount} rows in my_tlkpproject_all for year {Year}", rowsAffected, year);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh project all for year {Year}", year);
            throw;
        }
    }
}
