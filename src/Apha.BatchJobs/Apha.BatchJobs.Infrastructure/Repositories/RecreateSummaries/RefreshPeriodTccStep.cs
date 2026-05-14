using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class RefreshPeriodTccStep : RecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public RefreshPeriodTccStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodTcc";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        await db.RsPeriodTimeCostCalcs
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await (
            from tcc in db.RsTimeCostCalcs.AsNoTracking()
            join wg in db.RsWorkGroup.AsNoTracking() on tcc.WorkGroup equals wg.WorkGroup
            join p in db.RsTlkpProject.AsNoTracking() on tcc.Project equals p.ParentProject
            join cc0 in db.RsCostCentre.AsNoTracking() on p.CostCentre equals cc0.CostCentre into cc1
            from cc in cc1.DefaultIfEmpty()
            join emp in db.RsTblWgEmployee.AsNoTracking() on tcc.StaffId equals emp.PactId
            select new RsPeriodTimeCostCalcsTable
            {
                Period = _period,
                Project = p.ParentProject,
                OracleProjectCode = p.OracleProjectCode,
                SubAccountCode = p.SubAccountCode,
                Month = tcc.Month,
                DefraProject = (p.IsDefraProject ?? 0) == 0 ? "No" : "Yes",
                Occ = cc != null ? cc.CostCentre : null,
                Opc = cc != null ? cc.ProfitCentre : null,
                Spc = wg.ProfitCentre,
                Scc = wg.CostCentre,
                Name = tcc.Name ?? string.Empty,
                GradeCode = tcc.GradeCode,
                SpNumber = emp.SpNumber,
                ChargeRate = tcc.ChargeRate,
                Pay = tcc.Pay,
                NonPay = tcc.NonPay,
                Overhead = tcc.Overhead,
                Time = tcc.Time,
                TotalCost = tcc.Cost
            })
            .ToListAsync(cancellationToken);

        await db.RsPeriodTimeCostCalcs.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
