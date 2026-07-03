using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class RefreshPeriodMoStep : RecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public RefreshPeriodMoStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodMo";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var fpsYear = context.FpsYear;

        await db.RsPeriodMonthlyOutput
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await (
            from mo in db.RsMonthlyOutput.AsNoTracking()
            join wg in db.RsWorkGroup.AsNoTracking() on mo.WorkGroup equals wg.WorkGroup
            join tr in db.RsTlkpTestReqmt.AsNoTracking()
                on new { Buyer = mo.Buyer, mo.TestCode }
                equals new { Buyer = tr.ProjectBuyerCode, tr.TestCode }
            join p in db.RsTlkpProject.AsNoTracking() on mo.Buyer equals p.ParentProject
            where p.FpsYear == fpsYear
            join cc0 in db.RsCostCentre.AsNoTracking()
                on new { CostCentre = p.CostCentre, FpsYear = p.FpsYear }
                equals new { CostCentre = (double?)cc0.CostCentre, cc0.FpsYear } into cc1
            from cc in cc1.DefaultIfEmpty()
            select new RsPeriodMonthlyOutputTable
            {
                Period = _period,
                Project = p.ParentProject,
                OracleProjectCode = p.OracleProjectCode,
                SubAccountCode = p.SubAccountCode,
                IsDefraProject = (p.IsDefraProject ?? 0) == 0 ? "No" : "Yes",
                Opc = cc != null ? cc.ProfitCentre : null,
                Occ = cc != null ? cc.CostCentre : null,
                Month = mo.Month,
                Spc = wg.ProfitCentre,
                WorkGroup = wg.WorkGroup,
                Scc = wg.CostCentre,
                TestCode = mo.TestCode,
                Volume = mo.Volume,
                TestPrice = tr.UnitPrice,
                TotalCost = tr.UnitPrice * (decimal?)mo.Volume
            })
            .ToListAsync(cancellationToken);

        await db.RsPeriodMonthlyOutput.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
