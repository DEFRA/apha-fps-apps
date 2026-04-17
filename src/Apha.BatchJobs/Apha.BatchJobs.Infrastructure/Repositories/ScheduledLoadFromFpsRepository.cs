using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// SQL-backed repository for ScheduledLoadFromFps execution and audit operations.
/// </summary>
public sealed class ScheduledLoadFromFpsRepository : IScheduledLoadFromFpsRepository
{
    private readonly BatchJobsDbContext _context;

    public ScheduledLoadFromFpsRepository(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> StartRunAsync(string jobName, int fpsYear, string correlationId, CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid();

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.scheduled_load_run (
    run_id,
    job_name,
    fps_year,
    job_started_at,
    final_status,
    correlation_id,
    created_at
)
VALUES (
    {runId},
    {jobName},
    {fpsYear},
    NOW(),
    'Running',
    {correlationId},
    NOW()
)
", cancellationToken);

        return runId;
    }

    public Task CompleteRunAsync(Guid runId, string finalStatus, CancellationToken cancellationToken)
    {
        return _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE fps.scheduled_load_run
SET
    job_completed_at = NOW(),
    final_status = {finalStatus}
WHERE run_id = {runId}
", cancellationToken);
    }

    public async Task<Guid> StartStepAsync(
        Guid runId,
        ScheduledLoadFromFpsStep step,
        int stepSequence,
        CancellationToken cancellationToken)
    {
        var stepRunId = Guid.NewGuid();

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.scheduled_load_step_run (
    step_run_id,
    run_id,
    step_name,
    step_sequence,
    started_at,
    step_status,
    created_at
)
VALUES (
    {stepRunId},
    {runId},
    {step.ToString()},
    {stepSequence},
    NOW(),
    'Running',
    NOW()
)
", cancellationToken);

        return stepRunId;
    }

    public Task CompleteStepAsync(
        Guid stepRunId,
        string stepStatus,
        int? rowsAffected,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        return _context.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE fps.scheduled_load_step_run
SET
    completed_at = NOW(),
    step_status = {stepStatus},
    rows_affected = {rowsAffected},
    error_message = {errorMessage}
WHERE step_run_id = {stepRunId}
", cancellationToken);
    }

    public async Task<int> RebuildYearTotalsAsync(int year, CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM fps.fpsyeartotals
WHERE fpsyear = {year}
", cancellationToken);

        return await _context.Database.ExecuteSqlInterpolatedAsync($@"
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
    totaladditionalcosts = EXCLUDED.totaladditionalcosts,
    totalanimalcosts = EXCLUDED.totalanimalcosts,
    totalstaffcosts = EXCLUDED.totalstaffcosts,
    totaltestcosts = EXCLUDED.totaltestcosts,
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
    totalpaycosts = EXCLUDED.totalpaycosts,
    fpsyear = EXCLUDED.fpsyear
", cancellationToken);
    }

    public async Task<int> DeleteArchiveYearSliceAsync(int year, CancellationToken cancellationToken)
    {
        var totalRowsAffected = 0;

        totalRowsAffected += await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_fpsyeartotals
WHERE year = {year}
", cancellationToken);

        totalRowsAffected += await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject_all
WHERE year = {year}
", cancellationToken);

        totalRowsAffected += await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject
WHERE year = {year}
", cancellationToken);

        return totalRowsAffected;
    }

    public async Task<int> AddArchiveYearSliceAsync(int year, CancellationToken cancellationToken)
    {
        var totalRowsAffected = 0;

        totalRowsAffected += await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_fpsyeartotals (
    year,
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
    totalpaycosts
)
SELECT
    {year},
    f.parentproject,
    f.program,
    f.totaladditionalcosts,
    f.totalanimalcosts,
    f.totalstaffcosts,
    f.totaltestcosts,
    f.totalcosts,
    f.custincome,
    f.transferincome,
    f.totalincome,
    f.budget_cvl,
    f.requiredprofit,
    f.manager,
    f.customer,
    COALESCE(f.projectstatus, 'Active'),
    f.pvsincome,
    f.plancaseworkdebit,
    f.totalpaycosts
FROM fps.fpsyeartotals f
WHERE f.fpsyear = {year}
ON CONFLICT (year, parentproject) DO UPDATE
SET
    program = EXCLUDED.program,
    totaladditionalcosts = EXCLUDED.totaladditionalcosts,
    totalanimalcosts = EXCLUDED.totalanimalcosts,
    totalstaffcosts = EXCLUDED.totalstaffcosts,
    totaltestcosts = EXCLUDED.totaltestcosts,
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

        totalRowsAffected += await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject (
    year,
    parentproject,
    program,
    customer,
    manager,
    transferincome,
    custincome,
    wip_eoy,
    wip_limit,
    wip_current,
    projectstatus,
    datecreated,
    feccost,
    profit,
    budget_cvl,
    caseworksub,
    pvsincome,
    plancaseworkdebit,
    source,
    disease,
    contract,
    finished,
    comments,
    carryover,
    isdefraproject,
    costcentre,
    oracleprojectcode,
    subaccountcode,
    projectgroup,
    incomeaccountcode
)
SELECT
    {year},
    LEFT(t.parentproject::text, 20),
    LEFT(t.program::text, 10),
    LEFT(t.customer::text, 50),
    t.manager,
    t.transferincome,
    t.custincome,
    t.wip_eoy,
    t.wip_limit,
    t.wip_current,
    LEFT(t.projectstatus::text, 50),
    t.datecreated,
    t.feccost,
    t.profit,
    t.budget_cvl,
    t.caseworksub,
    t.pvsincome,
    t.plancaseworkdebit,
    'FPS',
    LEFT(t.disease::text, 50),
    LEFT(t.contract::text, 10),
    t.finished,
    t.comments,
    t.carryover,
    t.isdefraproject,
    t.costcentre,
    t.oracleprojectcode,
    LEFT(t.subaccountcode::text, 50),
    LEFT(t.projectgroup::text, 50),
    LEFT(t.incomeaccountcode::text, 50)
FROM fps.tlkpproject t
WHERE t.fpsyear = {year}
ON CONFLICT (year, parentproject) DO UPDATE
SET
    program = EXCLUDED.program,
    customer = EXCLUDED.customer,
    manager = EXCLUDED.manager,
    transferincome = EXCLUDED.transferincome,
    custincome = EXCLUDED.custincome,
    wip_eoy = EXCLUDED.wip_eoy,
    wip_limit = EXCLUDED.wip_limit,
    wip_current = EXCLUDED.wip_current,
    projectstatus = EXCLUDED.projectstatus,
    datecreated = EXCLUDED.datecreated,
    feccost = EXCLUDED.feccost,
    profit = EXCLUDED.profit,
    budget_cvl = EXCLUDED.budget_cvl,
    caseworksub = EXCLUDED.caseworksub,
    pvsincome = EXCLUDED.pvsincome,
    plancaseworkdebit = EXCLUDED.plancaseworkdebit,
    source = EXCLUDED.source,
    disease = EXCLUDED.disease,
    contract = EXCLUDED.contract,
    finished = EXCLUDED.finished,
    comments = EXCLUDED.comments,
    carryover = EXCLUDED.carryover,
    isdefraproject = EXCLUDED.isdefraproject,
    costcentre = EXCLUDED.costcentre,
    oracleprojectcode = EXCLUDED.oracleprojectcode,
    subaccountcode = EXCLUDED.subaccountcode,
    projectgroup = EXCLUDED.projectgroup,
    incomeaccountcode = EXCLUDED.incomeaccountcode
", cancellationToken);

        return totalRowsAffected;
    }

    public async Task<int> RefreshCurrentYearProjectAllAsync(int currentYear, CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.my_tlkpproject_all
WHERE year = {currentYear}
", cancellationToken);

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
DELETE FROM mabarchive.g_tlkpproject g
WHERE EXISTS (
    SELECT 1
    FROM fps.tlkpproject t
    WHERE t.fpsyear = {currentYear}
      AND LEFT(t.parentproject::text, 20) = g.parentproject
)
", cancellationToken);

        var allRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkpproject_all (
    year,
    parentproject,
    program,
    customer,
    manager,
    transferincome,
    custincome,
    wip_eoy,
    wip_limit,
    wip_current,
    projectstatus,
    datecreated,
    feccost,
    profit,
    budget_cvl,
    caseworksub,
    pvsincome,
    plancaseworkdebit,
    source,
    disease,
    contract,
    finished,
    comments,
    carryover,
    isdefraproject,
    costcentre,
    oracleprojectcode,
    subaccountcode,
    projectgroup,
    incomeaccountcode
)
SELECT
    {currentYear},
    LEFT(t.parentproject::text, 20),
    LEFT(t.program::text, 10),
    LEFT(t.customer::text, 50),
    t.manager,
    t.transferincome,
    t.custincome,
    t.wip_eoy,
    t.wip_limit,
    t.wip_current,
    LEFT(t.projectstatus::text, 50),
    t.datecreated::date,
    t.feccost,
    t.profit,
    t.budget_cvl,
    t.caseworksub,
    t.pvsincome,
    t.plancaseworkdebit,
    'FPS',
    LEFT(t.disease::text, 50),
    LEFT(t.contract::text, 10),
    t.finished,
    t.comments,
    t.carryover,
    t.isdefraproject,
    t.costcentre,
    t.oracleprojectcode,
    LEFT(t.subaccountcode::text, 50),
    LEFT(t.projectgroup::text, 50),
    LEFT(t.incomeaccountcode::text, 50)
FROM fps.tlkpproject t
WHERE t.fpsyear = {currentYear}
ON CONFLICT (year, parentproject) DO UPDATE
SET
    program = EXCLUDED.program,
    customer = EXCLUDED.customer,
    manager = EXCLUDED.manager,
    transferincome = EXCLUDED.transferincome,
    custincome = EXCLUDED.custincome,
    wip_eoy = EXCLUDED.wip_eoy,
    wip_limit = EXCLUDED.wip_limit,
    wip_current = EXCLUDED.wip_current,
    projectstatus = EXCLUDED.projectstatus,
    datecreated = EXCLUDED.datecreated,
    feccost = EXCLUDED.feccost,
    profit = EXCLUDED.profit,
    budget_cvl = EXCLUDED.budget_cvl,
    caseworksub = EXCLUDED.caseworksub,
    pvsincome = EXCLUDED.pvsincome,
    plancaseworkdebit = EXCLUDED.plancaseworkdebit,
    source = EXCLUDED.source,
    disease = EXCLUDED.disease,
    contract = EXCLUDED.contract,
    finished = EXCLUDED.finished,
    comments = EXCLUDED.comments,
    carryover = EXCLUDED.carryover,
    isdefraproject = EXCLUDED.isdefraproject,
    costcentre = EXCLUDED.costcentre,
    oracleprojectcode = EXCLUDED.oracleprojectcode,
    subaccountcode = EXCLUDED.subaccountcode,
    projectgroup = EXCLUDED.projectgroup,
    incomeaccountcode = EXCLUDED.incomeaccountcode
", cancellationToken);

        var gRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.g_tlkpproject (
    parentproject,
    projecttitle,
    costbookno,
    disease,
    contract,
    shorttitle,
    projectstatus
)
SELECT
    LEFT(t.parentproject::text, 20),
    t.projecttitle,
    t.costbookno,
    LEFT(t.disease::text, 50),
    LEFT(t.contract::text, 10),
    t.shorttitle,
    LEFT(t.projectstatus::text, 50)
FROM fps.tlkpproject t
WHERE t.fpsyear = {currentYear}
ON CONFLICT (parentproject) DO UPDATE
SET
    projecttitle = EXCLUDED.projecttitle,
    costbookno = EXCLUDED.costbookno,
    disease = EXCLUDED.disease,
    contract = EXCLUDED.contract,
    shorttitle = EXCLUDED.shorttitle,
    projectstatus = EXCLUDED.projectstatus
", cancellationToken);

        return allRows + gRows;
    }

    public async Task<IReadOnlyList<ScheduledLoadValidationAssertionResult>> RunCrossValidationAsync(
        Guid runId,
        ScheduledLoadFromFpsExecutionContext context,
        int expectedStepCount,
        CancellationToken cancellationToken)
    {
        var includeCurrentYear = context.CurrentMonth > context.CurrentYearCutoverMonth;
        var currentYear = context.CurrentYear;
        var previousYear = context.PreviousYear;

        var results = new List<ScheduledLoadValidationAssertionResult>
        {
            await BuildAssertionAsync(
                runId,
                "ASSERT_001",
                "Row count parity between fps totals and archive totals for current year",
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.fpsyeartotals
WHERE fpsyear = {currentYear}
", cancellationToken),
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM mabarchive.my_fpsyeartotals
WHERE year = {currentYear}
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_002",
                "No null parentproject values in archive totals",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM mabarchive.my_fpsyeartotals
WHERE year IN ({previousYear}, {currentYear})
  AND parentproject IS NULL
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_003",
                "totalcosts formula parity in fps.fpsyeartotals",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.fpsyeartotals
WHERE fpsyear IN ({previousYear}, {currentYear})
  AND COALESCE(totalcosts, 0) <> COALESCE(totaladditionalcosts, 0)::double precision
      + COALESCE(totalanimalcosts, 0)
      + COALESCE(totalstaffcosts, 0)
      + COALESCE(totaltestcosts, 0)
      + COALESCE(plancaseworkdebit, 0)::double precision
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_004",
                "totalincome formula parity in fps.fpsyeartotals",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.fpsyeartotals
WHERE fpsyear IN ({previousYear}, {currentYear})
  AND COALESCE(totalincome, 0)::numeric <> COALESCE(custincome, 0)::numeric + COALESCE(transferincome, 0)::numeric
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_005",
                "Archive totals rows are constrained to expected years",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM mabarchive.my_fpsyeartotals
WHERE year NOT IN ({previousYear}, {currentYear})
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_006",
                "(year,parentproject) remains unique in archive totals",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM (
    SELECT year, parentproject, COUNT(*)
    FROM mabarchive.my_fpsyeartotals
    WHERE year IN ({previousYear}, {currentYear})
    GROUP BY year, parentproject
    HAVING COUNT(*) > 1
) dupes
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_007",
                "Archive footprint has totals rows for previous year",
                1m,
                await ScalarDecimalAsync($@"
SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END::numeric
FROM mabarchive.my_fpsyeartotals
WHERE year = {previousYear}
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_008",
                "Archive totals parentproject set is represented in fps totals for current year",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM mabarchive.my_fpsyeartotals a
WHERE a.year = {currentYear}
  AND NOT EXISTS (
      SELECT 1
      FROM fps.fpsyeartotals f
      WHERE f.fpsyear = {currentYear}
        AND f.parentproject = a.parentproject
  )
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_009",
                "Current year project snapshot exists",
                1m,
                await ScalarDecimalAsync($@"
SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END::numeric
FROM mabarchive.my_tlkpproject_all
WHERE year = {currentYear}
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_010",
                "Current year snapshot parentproject values exist in source tlkpproject",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM mabarchive.my_tlkpproject_all a
WHERE a.year = {currentYear}
  AND NOT EXISTS (
      SELECT 1
      FROM fps.tlkpproject t
      WHERE t.fpsyear = {currentYear}
        AND LEFT(t.parentproject::text, 20) = a.parentproject
  )
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_011",
                "Numeric range checks: totalcosts > 0 and totalincome >= 0",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.fpsyeartotals
WHERE fpsyear IN ({previousYear}, {currentYear})
  AND (COALESCE(totalcosts, 0) <= 0 OR COALESCE(totalincome, 0)::numeric < 0)
", cancellationToken),
                cancellationToken),

            await BuildAssertionAsync(
                runId,
                "ASSERT_012",
                "Step audit records count equals executed step count",
                expectedStepCount,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.scheduled_load_step_run
WHERE run_id = {runId}
", cancellationToken),
                cancellationToken)
        };

        if (!includeCurrentYear)
        {
            results.Add(await BuildAssertionAsync(
                runId,
                "ASSERT_013",
                "Pre-cutover branch excludes current-year totals step",
                0m,
                await ScalarDecimalAsync($@"
SELECT COUNT(*)::numeric
FROM fps.scheduled_load_step_run
WHERE run_id = {runId}
  AND step_name = 'ProcessCurrentYearTotals'
", cancellationToken),
                cancellationToken));
        }

        return results;
    }

    private async Task<ScheduledLoadValidationAssertionResult> BuildAssertionAsync(
        Guid runId,
        string assertionCode,
        string assertionDescription,
        decimal expected,
        decimal actual,
        CancellationToken cancellationToken)
    {
        var passed = expected == actual;
        var errorMessage = passed
            ? null
            : $"Expected {expected} but got {actual}.";

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.scheduled_load_validation_result (
    validation_id,
    run_id,
    assertion_code,
    assertion_description,
    expected_value,
    actual_value,
    passed,
    error_message,
    checked_at,
    created_at
)
VALUES (
    gen_random_uuid(),
    {runId},
    {assertionCode},
    {assertionDescription},
    {expected},
    {actual},
    {passed},
    {errorMessage},
    NOW(),
    NOW()
)
ON CONFLICT (run_id, assertion_code)
DO UPDATE SET
    assertion_description = EXCLUDED.assertion_description,
    expected_value = EXCLUDED.expected_value,
    actual_value = EXCLUDED.actual_value,
    passed = EXCLUDED.passed,
    error_message = EXCLUDED.error_message,
    checked_at = EXCLUDED.checked_at
", cancellationToken);

        return new ScheduledLoadValidationAssertionResult(
            assertionCode,
            assertionDescription,
            expected,
            actual,
            passed,
            errorMessage);
    }

    private async Task<decimal> ScalarDecimalAsync(FormattableString sql, CancellationToken cancellationToken)
    {
        var result = await _context.Database.SqlQueryRaw<decimal>(sql.Format, sql.GetArguments()).SingleAsync(cancellationToken);
        return result;
    }
}
