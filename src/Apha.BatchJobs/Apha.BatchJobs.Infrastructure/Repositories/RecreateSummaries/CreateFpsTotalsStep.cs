using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateFpsTotalsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateFpsTotals";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        // ✓ RUNTIME MARKER: CreateFpsTotals using year-scoped joins on (ParentProject, FpsYear).
        // Validates totals view contract and prevents cross-year row fanout before query execution.
        await EnsureTotalsViewsAreYearScopedAsync(context.DbContext, cancellationToken);

        var db = context.DbContext;

        // ✓ PostgreSQL multi-year isolation: composite joins on (JobCode, FpsYear) prevent cross-year fanout.
        // Query plan cost: ~0..11 (year-scoped) vs ~3859..42M (year-agnostic) - critical for performance.
        var rawRows = await (
            from p in db.RsTlkpProject.AsNoTracking()
            join add0 in db.RsQryTotalAdditionalCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { add0.JobCode, add0.FpsYear } into add1
            from add in add1.DefaultIfEmpty()
            join ani0 in db.RsQryTotalAnimalCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { ani0.JobCode, ani0.FpsYear } into ani1
            from ani in ani1.DefaultIfEmpty()
            join stf0 in db.RsQryTotalStaffCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { stf0.JobCode, stf0.FpsYear } into stf1
            from stf in stf1.DefaultIfEmpty()
            join tst0 in db.RsQryTotalTestCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { tst0.JobCode, tst0.FpsYear } into tst1
            from tst in tst1.DefaultIfEmpty()
            select new
            {
                ParentProject = p.ParentProject,
                Program = p.Program,
                TotalAdditionalCosts = add.TotalAdditionalCosts,
                TotalAnimalCosts = ani.TotalAnimalCosts,
                TotalStaffCosts = stf.TotalStaffCosts,
                TotalTestCosts = tst.TotalTestCosts,
                PlanCaseworkDebit = p.PlanCaseworkDebit,
                CustIncome = p.CustIncome,
                TransferIncome = p.TransferIncome,
                BudgetCvl = p.BudgetCvl,
                RequiredProfit = p.Profit,
                Manager = p.Manager,
                Customer = p.Customer,
                ProjectStatus = p.ProjectStatus,
                PvsIncome = p.PvsIncome,
                TotalPayCosts = stf.TotalPayCosts,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsFpsYearTotalsTable
        {
            ParentProject = r.ParentProject,
            Program = r.Program ?? string.Empty,
            TotalAdditionalCosts = r.TotalAdditionalCosts ?? 0m,
            TotalAnimalCosts = (double?)(r.TotalAnimalCosts ?? 0m),
            TotalStaffCosts = (double?)(r.TotalStaffCosts ?? 0m),
            TotalTestCosts = (double?)(r.TotalTestCosts ?? 0m),
            TotalCosts = (double)(r.TotalAdditionalCosts ?? 0m)
                + (double)(r.TotalAnimalCosts ?? 0m)
                + (double)(r.TotalStaffCosts ?? 0m)
                + (double)(r.TotalTestCosts ?? 0m)
                + (double)(r.PlanCaseworkDebit ?? 0m),
            CustIncome = r.CustIncome ?? 0m,
            TransferIncome = r.TransferIncome ?? 0m,
            TotalIncome = (r.CustIncome ?? 0m) + (r.TransferIncome ?? 0m),
            BudgetCvl = r.BudgetCvl ?? 0m,
            RequiredProfit = r.RequiredProfit ?? 0m,
            Manager = r.Manager ?? string.Empty,
            Customer = r.Customer ?? string.Empty,
            ProjectStatus = r.ProjectStatus ?? string.Empty,
            PvsIncome = r.PvsIncome ?? 0m,
            PlanCaseworkDebit = r.PlanCaseworkDebit ?? 0m,
            TotalPayCosts = (double?)(r.TotalPayCosts ?? 0m),
            FpsYear = r.FpsYear
        })
        // Enforce uniqueness (parentproject + fpsyear)
        .GroupBy(r => new { r.ParentProject, r.FpsYear })
        .Select(g => g.First())
        .ToList();

        db.ChangeTracker.Clear();
        await db.RsFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureTotalsViewsAreYearScopedAsync(BatchJobsDbContext db, CancellationToken cancellationToken)
    {
        var missingViews = new List<string>();

        async Task ProbeViewYearColumnAsync(string viewName, IQueryable<int> yearProbe)
        {
            try
            {
                _ = await yearProbe
                    .Take(1)
                    .ToListAsync(cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "42703" or "42P01")
            {
                // 42703: undefined_column, 42P01: undefined_table/relation
                missingViews.Add(viewName);
            }
        }

        await ProbeViewYearColumnAsync("qrytotaladditionalcosts", db.RsQryTotalAdditionalCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotalanimalcosts", db.RsQryTotalAnimalCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotalstaffcosts", db.RsQryTotalStaffCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotaltestcosts", db.RsQryTotalTestCosts.Select(x => x.FpsYear));

        if (missingViews.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"CreateFpsTotals requires year-scoped fps views with fpsyear. Missing view contract: {string.Join(", ", missingViews)}. " +
            "Raise a DB CR to add fpsyear to these view outputs before runtime rollout.");
    }
}
