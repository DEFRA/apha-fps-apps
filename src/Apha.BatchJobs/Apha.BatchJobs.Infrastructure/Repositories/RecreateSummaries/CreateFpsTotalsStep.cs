using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateFpsTotalsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateFpsTotals";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Two-step: fetch raw nullable values first (avoid COALESCE on PostgreSQL money columns),
        // then apply defaults in C#.
        var rawRows = await (
            from p in db.RsTlkpProject.AsNoTracking()
            join add0 in db.RsQryTotalAdditionalCosts.AsNoTracking() on p.ParentProject equals add0.JobCode into add1
            from add in add1.DefaultIfEmpty()
            join ani0 in db.RsQryTotalAnimalCosts.AsNoTracking() on p.ParentProject equals ani0.JobCode into ani1
            from ani in ani1.DefaultIfEmpty()
            join stf0 in db.RsQryTotalStaffCosts.AsNoTracking() on p.ParentProject equals stf0.JobCode into stf1
            from stf in stf1.DefaultIfEmpty()
            join tst0 in db.RsQryTotalTestCosts.AsNoTracking() on p.ParentProject equals tst0.JobCode into tst1
            from tst in tst1.DefaultIfEmpty()
            select new
            {
                ParentProject = p.ParentProject,
                Program = p.Program,
                AddCosts = add.TotalAdditionalCosts,
                AniCosts = ani.TotalAnimalCosts,
                StfCosts = stf.TotalStaffCosts,
                TstCosts = tst.TotalTestCosts,
                StfPayCosts = stf.TotalPayCosts,
                CustIncome = p.CustIncome,
                TransferIncome = p.TransferIncome,
                BudgetCvl = p.BudgetCvl,
                Profit = p.Profit,
                Manager = p.Manager,
                Customer = p.Customer,
                ProjectStatus = p.ProjectStatus,
                PvsIncome = p.PvsIncome,
                PlanCaseworkDebit = p.PlanCaseworkDebit,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsFpsYearTotalsTable
        {
            ParentProject = r.ParentProject,
            Program = r.Program,
            TotalAdditionalCosts = r.AddCosts ?? 0m,
            TotalAnimalCosts = (double?)(r.AniCosts) ?? 0d,
            TotalStaffCosts = (double?)(r.StfCosts) ?? 0d,
            TotalTestCosts = (double?)(r.TstCosts) ?? 0d,
            TotalCosts =
                ((double?)(r.AddCosts) ?? 0d) +
                ((double?)(r.AniCosts) ?? 0d) +
                ((double?)(r.StfCosts) ?? 0d) +
                ((double?)(r.TstCosts) ?? 0d) +
                ((double?)(r.PlanCaseworkDebit) ?? 0d),
            CustIncome = r.CustIncome,
            TransferIncome = r.TransferIncome,
            TotalIncome = r.CustIncome + r.TransferIncome,
            BudgetCvl = r.BudgetCvl,
            RequiredProfit = r.Profit,
            Manager = r.Manager,
            Customer = r.Customer,
            ProjectStatus = r.ProjectStatus,
            PvsIncome = r.PvsIncome ?? 0m,
            PlanCaseworkDebit = r.PlanCaseworkDebit ?? 0m,
            TotalPayCosts = (double?)(r.StfPayCosts) ?? 0d,
            FpsYear = r.FpsYear
        }).ToList();

        await db.RsFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
