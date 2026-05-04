using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IMyFpsYearlyDataService.
/// Manages yearly FPS archive data operations (delete, load, refresh).
/// Contract: delete/load/refresh operations are designed to run inside the orchestration transaction
/// provided by the caller so the full year cycle remains atomic.
/// </summary>
public sealed class MyFpsYearlyDataService : IMyFpsYearlyDataService
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<MyFpsYearlyDataService> _logger;
    private const int TotalLoaders = 24;

    private static readonly string[] ArchiveDeleteTables =
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
) AS ""Value""
").SingleAsync(cancellationToken);

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
    /// Must be executed inside the caller's orchestration transaction.
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
            foreach (var table in ArchiveDeleteTables)
            {
                _logger.LogInformation("Deleting table {TableName} for year {Year}", table, year);
                var deleteSql = $@"
DELETE FROM {table}
WHERE year = @year
";
                var deleteCount = await _context.Database.ExecuteSqlRawAsync(
                    deleteSql,
                    [new NpgsqlParameter("year", year)],
                    cancellationToken);

                totalRowsAffected += deleteCount;
                _logger.LogInformation("Deleted {RowCount} rows from {TableName} for year {Year}", deleteCount, table, year);
            }

            // Special handling for G_tlkpProject: project-based delete matching FPS source projects
            // (not year-based, but included in legacy scope per sp_DeleteYearsFPSData baseline)
            _logger.LogInformation("Deleting table mabarchive.g_tlkpproject using project keys for year {Year}", year);
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
    /// Implements full legacy sp_AddYearsFPSData fan-out in the exact same 24-loader execution order.
    /// All inserts are insert-only (no upsert); delete-then-insert is the idempotency mechanism per Assumption A3.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows loaded.</returns>
    public async Task<int> LoadYearDataAsync(int year, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading archive data for year {Year} — full sp_AddYearsFPSData fan-out (24 loaders, legacy parity order)", year);

        var currentLoaderNumber = 0;
        var currentLoaderName = "NotStarted";

        try
        {
            var totalRowsAffected = 0;
            int rowCount;

            void StartLoader(int loaderNumber, string loaderName)
            {
                currentLoaderNumber = loaderNumber;
                currentLoaderName = loaderName;
                _logger.LogInformation("[{LoaderNumber}/{TotalLoaders}] Starting {LoaderName} for year {Year}", loaderNumber, TotalLoaders, loaderName, year);
            }

            // ── Loader 1: sp_AddMY_tlkpProgram ─────────────────────────────────────────
            StartLoader(1, "my_tlkpprogram");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpprogram (
    year, programno, programname, directorate, minim, sector_name, customer, target, manager
)
SELECT
    {year}, p.programno, p.programname, p.directorate, p.minim, p.sector_name,
    p.customer, p.target, p.manager
FROM fps.tlkpprogram p
WHERE p.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[1/24] my_tlkpprogram: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 2: sp_AddG_tlkpProject ──────────────────────────────────────────
            StartLoader(2, "g_tlkpproject");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.g_tlkpproject (
    parentproject, projecttitle, costbookno, disease, contract, shorttitle, projectstatus
)
SELECT
    t.parentproject, t.projecttitle, t.costbookno, t.disease, t.contract,
    t.shorttitle, t.projectstatus
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
GROUP BY t.parentproject, t.projecttitle, t.costbookno, t.disease,
         t.contract, t.shorttitle, t.projectstatus
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[2/24] g_tlkpproject: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 3: sp_AddMY_tlkpProject ─────────────────────────────────────────
            // source column exists in DDL but legacy sp_AddMY_tlkpProject does not populate it
            StartLoader(3, "my_tlkpproject");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {year}, t.parentproject, t.program, t.customer, t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, t.projectstatus, t.datecreated, t.feccost,
    t.profit, t.budget_cvl, t.caseworksub, t.pvsincome, t.plancaseworkdebit,
    t.disease, t.contract, t.finished, t.comments, t.carryover, t.isdefraproject,
    t.costcentre, t.oracleprojectcode, t.subaccountcode, t.projectgroup, t.incomeaccountcode
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[3/24] my_tlkpproject: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 4: sp_AddMY_FPSYearTotals ───────────────────────────────────────
            // Plain copy from fps.fpsyeartotals — no defaults applied (legacy does not set any)
            StartLoader(4, "my_fpsyeartotals");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
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
    f.projectstatus, f.pvsincome, f.plancaseworkdebit, f.totalpaycosts
FROM fps.fpsyeartotals f
WHERE f.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[4/24] my_fpsyeartotals: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 5: sp_AddMY_MonthlyOutput ───────────────────────────────────────
            StartLoader(5, "my_monthlyoutput");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_monthlyoutput (
    year, testcode, buyer, month, workgroup, volume, wgbuyer
)
SELECT
    {year}, m.testcode, m.buyer, m.month, m.workgroup, m.volume, m.wgbuyer
FROM fps.monthlyoutput m
WHERE m.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[5/24] my_monthlyoutput: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 6: sp_AddMY_MonthlyTime ─────────────────────────────────────────
            // column is pactstaffid (no underscore) per both DDL and fps.monthlytime source
            StartLoader(6, "my_monthlytime");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_monthlytime (
    year, pactstaffid, timecode, month, parentproject, workgroup, hours
)
SELECT
    {year}, m.pactstaffid, m.timecode, m.month, m.parentproject, m.workgroup, m.hours
FROM fps.monthlytime m
WHERE m.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[6/24] my_monthlytime: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 7: sp_AddMY_Proj_Invoice ────────────────────────────────────────
            StartLoader(7, "my_proj_invoice");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_proj_invoice (
    year, projectparent, month, amount, costofwork, wip, profitloss, detail,
    invoicecounter, type
)
SELECT
    {year}, i.projectparent, i.month, i.amount, i.costofwork, i.wip, i.profitloss,
    i.detail, i.invoicecounter, i.type
FROM fps.proj_invoice i
WHERE i.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[7/24] my_proj_invoice: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 8: sp_AddMY_Proj_SubContract ────────────────────────────────────
            StartLoader(8, "my_proj_subcontract");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_proj_subcontract (
    year, subcontcounter, project, testjob, month, amount, workgroup, acctcode,
    supplier, description, suppliernumber, dailyrate, animaldays
)
SELECT
    {year}, s.subcontcounter, s.project, s.testjob, s.month, s.amount, s.workgroup,
    s.acctcode, s.supplier, s.description, s.suppliernumber, s.dailyrate, s.animaldays
FROM fps.proj_subcontract s
WHERE s.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[8/24] my_proj_subcontract: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 9: sp_AddMY_ProjectMonthFinal ───────────────────────────────────
            // 36 columns per legacy SQL; fps.projectmonthfinal has an extra 'x' column — omitted
            StartLoader(9, "my_projectmonthfinal");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_projectmonthfinal (
    year, project, monthno, periodname, cumflag, costprofile, subcontracts, animals,
    nonanimals, timecosts, transfercosts, totalcost, invoices, coiw, portsales,
    cumcost, cumprofile, sumofcostprofile, cuminvoices, cumcoiw, cumportsales,
    mstonedue, due__done, ontime, sumofmstonedue, sumofdue__done, sumofontime,
    cwdebit, cwcredit, cumcwdebit, cumcwcredit, totalhours, cumtotalhours,
    cumsubcontracts, cumtestcosts, paycosts, cumpaycosts
)
SELECT
    {year}, p.project, p.monthno, p.periodname, p.cumflag, p.costprofile, p.subcontracts,
    p.animals, p.nonanimals, p.timecosts, p.transfercosts, p.totalcost, p.invoices,
    p.coiw, p.portsales, p.cumcost, p.cumprofile, p.sumofcostprofile, p.cuminvoices,
    p.cumcoiw, p.cumportsales, p.mstonedue, p.due__done, p.ontime, p.sumofmstonedue,
    p.sumofdue__done, p.sumofontime, p.cwdebit, p.cwcredit, p.cumcwdebit, p.cumcwcredit,
    p.totalhours, p.cumtotalhours, p.cumsubcontracts, p.cumtestcosts, p.paycosts, p.cumpaycosts
FROM fps.projectmonthfinal p
WHERE p.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[9/24] my_projectmonthfinal: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 10: sp_AddMY_tblAdditionalCosts ─────────────────────────────────
            StartLoader(10, "my_tbladditionalcosts");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tbladditionalcosts (
    year, jobcode, account, description, itemcost, freq, supplier, ac_counter
)
SELECT
    {year}, a.jobcode, a.account, a.description, a.itemcost, a.freq, a.supplier,
    ROW_NUMBER() OVER (ORDER BY a.jobcode, a.account, a.description)
        + COALESCE((SELECT MAX(ac_counter) FROM mabarchive.my_tbladditionalcosts), 0)
FROM fps.tbladditionalcosts a
WHERE a.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[10/24] my_tbladditionalcosts: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 11: sp_AddMY_tblAnimalReq ───────────────────────────────────────
            StartLoader(11, "my_tblanimalreq");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tblanimalreq (
    year, jobcode, animaltype, numberofdays, numberofanimals
)
SELECT
    {year}, a.jobcode, a.animaltype, a.numberofdays, a.numberofanimals
FROM fps.tblanimalreq a
WHERE a.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[11/24] my_tblanimalreq: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 12: sp_AddMY_tblContract ────────────────────────────────────────
            StartLoader(12, "my_tblcontract");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tblcontract (
    year, contractno, category, manager, customer, title,
    registereddate, startdate, enddate, contractdoc, duration
)
SELECT
    {year}, c.contractno, c.category, c.manager, c.customer, c.title,
    c.registereddate, c.startdate, c.enddate, c.contractdoc, c.duration
FROM fps.tblcontract c
WHERE c.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[12/24] my_tblcontract: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 13: sp_AddMY_tblStaffJob ────────────────────────────────────────
            // systimestamp (bytea) column in target DDL not populated by legacy procedure — left NULL
            StartLoader(13, "my_tblstaffjob");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tblstaffjob (
    year, staffid, jobcode, plannedhours
)
SELECT
    {year}, s.staffid, s.jobcode, s.plannedhours
FROM fps.tblstaffjob s
WHERE s.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[13/24] my_tblstaffjob: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 14: sp_AddMY_TimeCostCalcs ──────────────────────────────────────
            // All 16 source columns per legacy SQL
            StartLoader(14, "my_timecostcalcs");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_timecostcalcs (
    year, workgroup, jobcode, project, month, staffid,
    gradecode, name, chargerate, class, time, cost,
    division, jobcodeold, pay, nonpay, overhead
)
SELECT
    {year}, t.workgroup, t.jobcode, t.project, t.month, t.staffid,
    t.gradecode, t.name, t.chargerate, t.class, t.time, t.cost,
    t.division, t.jobcodeold, t.pay, t.nonpay, t.overhead
FROM fps.timecostcalcs t
WHERE t.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[14/24] my_timecostcalcs: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 15: sp_AddMY_tlkpTestReqmt ──────────────────────────────────────
            // source column in target DDL not populated by legacy procedure — left NULL
            StartLoader(15, "my_tlkptestreqmt");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkptestreqmt (
    year, testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode
)
SELECT
    {year}, r.testcode, r.buyer, r.unitprice, r.norequired, r.projectbuyercode, r.testbuyercode
FROM fps.tlkptestreqmt r
WHERE r.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[15/24] my_tlkptestreqmt: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 16: sp_addMY_YearDetails ────────────────────────────────────────
            // fps.tbldb_variables has no fpsyear; reads the shared current-month setting
            StartLoader(16, "tlkpyear");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.tlkpyear (year, latestmonthreleased)
SELECT {year}, CAST(v.db_var_value AS integer)
FROM fps.tbldb_variables v
WHERE v.db_var_name = 'month'
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[16/24] tlkpyear: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 17: sp_addMY_WorkGroupGrade ─────────────────────────────────────
            StartLoader(17, "my_workgroupgrade");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_workgroupgrade (
    year, wggrade, profitcentregrade, gradecode, workgroup
)
SELECT
    {year}, w.wggrade, w.profitcentregrade, w.gradecode, w.workgroup
FROM fps.workgroupgrade w
WHERE w.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[17/24] my_workgroupgrade: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 18: sp_addMY_ProfitCentreGrade ──────────────────────────────────
            StartLoader(18, "my_profitcentregrade");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_profitcentregrade (
    year, pcgrade, divisiongrade, gradecode, profitcentre,
    chargerate, directrate, payrate, npr, ohr
)
SELECT
    {year}, p.pcgrade, p.divisiongrade, p.gradecode, p.profitcentre,
    p.chargerate, p.directrate, p.payrate, p.npr, p.ohr
FROM fps.profitcentregrade p
WHERE p.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[18/24] my_profitcentregrade: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 19: sp_AddMY_tblProfitCentre ────────────────────────────────────
            // fps.tblkpprofitcentre has no fpsyear column (shared reference); copy all rows
            StartLoader(19, "my_tblprofitcentre");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tblprofitcentre (
    year, profitcentre, profitcentrename, division, conttarget, profitcentrehead, divisionid
)
SELECT
    {year}, p.profitcentre, p.profitcentrename, p.division,
    p.conttarget, p.profitcentrehead, p.divisionid
FROM fps.tblkpprofitcentre p
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[19/24] my_tblprofitcentre: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 20: sp_AddMY_TestOrProduct ──────────────────────────────────────
            StartLoader(20, "my_testorproduct");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_testorproduct (
    year, itemcode, itemdescription, testmanager, jobstatus,
    unitpricevla, priceahvg, owner, chargemethod, shortdescription, defraunitprice
)
SELECT
    {year}, t.itemcode, t.itemdescription, t.testmanager, t.jobstatus,
    t.unitpricevla, t.priceahvg, t.owner, t.chargemethod, t.shortdescription, t.defraunitprice
FROM fps.testorproduct t
WHERE t.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[20/24] my_testorproduct: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 21: sp_AddMY_Staff ───────────────────────────────────────────────
            // Legacy applies a per-user WorkGroup/ProfitCentre security filter. In batch job
            // context there is no user principal. The current implementation intentionally
            // loads all staff for the requested year; retain unless business explicitly
            // requires restoring user-scoped filtering semantics.
            StartLoader(21, "my_staff");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_staff (
    year, staffid, name, workgroupgrade, title,
    personstatus, personclass, hrspaid, leave, sickspecial, hrsavail
)
SELECT
    {year},
    wge.pactid,
    COALESCE(e.lastname, '') || ', ' || COALESCE(e.firstname, ''),
    wge.workgroupgrade,
    e.title,
    wge.personstatus,
    wge.personclass,
    wge.hrspaid,
    wge.leave,
    wge.sickspecial,
    wge.hrsavail
FROM fps.tblwgemployee wge
JOIN fps.tblemployee e ON wge.spnumber = e.spnumber
WHERE wge.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[21/24] my_staff: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 22: sp_AddMY_Workgroup ───────────────────────────────────────────
            StartLoader(22, "my_workgroup");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_workgroup (
    year, workgroup, profitcentre, costcentre, owner,
    description, centraloverhead, sendemail, cos90, costcentreold, email_recipient
)
SELECT
    {year}, w.workgroup, w.profitcentre, w.costcentre, w.owner,
    w.description, w.centraloverhead, w.sendemail, w.cos90, w.costcentreold, w.email_recipient
FROM fps.workgroup w
WHERE w.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[22/24] my_workgroup: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 23: sp_AddMY_tblAnimals ─────────────────────────────────────────
            StartLoader(23, "my_tblanimals");
            rowCount = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tblanimals (
    year, animaltype, species, security_level, dailyrate, planbyweek, defradailyrate
)
SELECT
    {year}, a.animaltype, a.species, a.security_level, a.dailyrate, a.planbyweek, a.defradailyrate
FROM fps.tblanimals a
WHERE a.fpsyear = {year}
", cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[23/24] my_tblanimals: {RowCount} rows for year {Year}", rowCount, year);

            // ── Loader 24: sp_AddMY_tlkpProject_All ────────────────────────────────────
            // Same shape as my_tlkpproject; source column left NULL per legacy
            StartLoader(24, "my_tlkpproject_all");
            rowCount = await InsertMyTlkpProjectAllAsync(year, cancellationToken);
            totalRowsAffected += rowCount;
            _logger.LogInformation("[24/24] my_tlkpproject_all: {RowCount} rows for year {Year}", rowCount, year);

            _logger.LogInformation("LoadYearDataAsync complete: {TotalRowCount} total rows loaded for year {Year}", totalRowsAffected, year);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load archive data for year {Year} while executing loader [{LoaderNumber}/24] {LoaderName}",
                year,
                currentLoaderNumber,
                currentLoaderName);
            throw;
        }
    }

    /// <summary>
    /// Refreshes only the my_tlkpproject_all table for the specified year.
    /// Must be executed inside the caller's orchestration transaction.
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
            var deletedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject_all
WHERE year = {year}
", cancellationToken);
            _logger.LogInformation("Deleted {RowCount} rows in my_tlkpproject_all for year {Year} prior to refresh", deletedRows, year);

            // Reload fresh records (source column left NULL per legacy sp_AddMY_tlkpProject_All)
            var rowsAffected = await InsertMyTlkpProjectAllAsync(year, cancellationToken);

            _logger.LogInformation("Refreshed {RowCount} rows in my_tlkpproject_all for year {Year}", rowsAffected, year);
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh project all for year {Year}", year);
            throw;
        }
    }

    private Task<int> InsertMyTlkpProjectAllAsync(int year, CancellationToken cancellationToken)
    {
        return _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject_all (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {year}, t.parentproject, t.program, t.customer, t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, t.projectstatus, t.datecreated, t.feccost,
    t.profit, t.budget_cvl, t.caseworksub, t.pvsincome, t.plancaseworkdebit,
    t.disease, t.contract, t.finished, t.comments, t.carryover, t.isdefraproject,
    t.costcentre, t.oracleprojectcode, t.subaccountcode, t.projectgroup, t.incomeaccountcode
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
", cancellationToken);
    }
}
