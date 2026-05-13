using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqCreateFpsTotalsStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateFpsTotals";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await (
            from p in db.RsTlkpProject.AsNoTracking()
            join add0 in db.RsQryTotalAdditionalCosts.AsNoTracking() on p.ParentProject equals add0.JobCode into add1
            from add in add1.DefaultIfEmpty()
            join ani0 in db.RsQryTotalAnimalCosts.AsNoTracking() on p.ParentProject equals ani0.JobCode into ani1
            from ani in ani1.DefaultIfEmpty()
            join stf0 in db.RsQryTotalStaffCosts.AsNoTracking() on p.ParentProject equals stf0.JobCode into stf1
            from stf in stf1.DefaultIfEmpty()
            join tst0 in db.RsQryTotalTestCosts.AsNoTracking() on p.ParentProject equals tst0.JobCode into tst1
            from tst in tst1.DefaultIfEmpty()
            select new RsFpsYearTotalsTable
            {
                ParentProject = p.ParentProject,
                Program = p.Program,
                TotalAdditionalCosts = add != null ? add.TotalAdditionalCosts ?? 0m : 0m,
                TotalAnimalCosts = ani != null ? (double?)ani.TotalAnimalCosts ?? 0d : 0d,
                TotalStaffCosts = stf != null ? (double?)stf.TotalStaffCosts ?? 0d : 0d,
                TotalTestCosts = tst != null ? (double?)tst.TotalTestCosts ?? 0d : 0d,
                TotalCosts =
                    (add != null ? (double?)add.TotalAdditionalCosts ?? 0d : 0d) +
                    (ani != null ? (double?)ani.TotalAnimalCosts ?? 0d : 0d) +
                    (stf != null ? (double?)stf.TotalStaffCosts ?? 0d : 0d) +
                    (tst != null ? (double?)tst.TotalTestCosts ?? 0d : 0d) +
                    ((double?)p.PlanCaseworkDebit ?? 0d),
                CustIncome = p.CustIncome,
                TransferIncome = p.TransferIncome,
                TotalIncome = p.CustIncome + p.TransferIncome,
                BudgetCvl = p.BudgetCvl,
                RequiredProfit = p.Profit,
                Manager = p.Manager,
                Customer = p.Customer,
                ProjectStatus = p.ProjectStatus,
                PvsIncome = p.PvsIncome ?? 0m,
                PlanCaseworkDebit = p.PlanCaseworkDebit ?? 0m,
                TotalPayCosts = stf != null ? (double?)stf.TotalPayCosts ?? 0d : 0d,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        await db.RsFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
