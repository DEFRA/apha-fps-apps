using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqRefreshPeriodPscStep : LinqRecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public LinqRefreshPeriodPscStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodPsc";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        await db.RsPeriodProjSubContract
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await (
            from psc in db.RsProjSubContract.AsNoTracking()
            join p in db.RsTlkpProject.AsNoTracking() on psc.Project equals p.ParentProject
            join cc0 in db.RsCostCentre.AsNoTracking() on p.CostCentre equals cc0.CostCentre into cc1
            from cc in cc1.DefaultIfEmpty()
            select new RsPeriodProjSubContractTable
            {
                Period = _period,
                SubContCounter = psc.SubContCounter,
                Project = psc.Project,
                OracleProjectCode = p.OracleProjectCode,
                SubAccountCode = p.SubAccountCode,
                IsDefraProject = (p.IsDefraProject ?? 0) == 0 ? "No" : "Yes",
                Opc = cc != null ? cc.ProfitCentre : null,
                Occ = cc != null ? cc.CostCentre : null,
                Month = psc.Month,
                Amount = psc.Amount,
                AcctCode = psc.AcctCode
            })
            .ToListAsync(cancellationToken);

        await db.RsPeriodProjSubContract.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
