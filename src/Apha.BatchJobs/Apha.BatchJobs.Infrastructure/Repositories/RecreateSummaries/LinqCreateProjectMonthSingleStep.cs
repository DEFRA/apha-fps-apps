using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqCreateProjectMonthSingleStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthSingle";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await (
            from pm in db.RsProjectMonth.AsNoTracking()
            join sc0 in db.RsQryJobMonthSubContracts.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { sc0.Project, sc0.Month } into sc1
            from sc in sc1.DefaultIfEmpty()
            join tm0 in db.RsQryJobMonthTime.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { tm0.Project, tm0.Month } into tm1
            from tm in tm1.DefaultIfEmpty()
            join ms0 in db.RsQryJobMonthMilestone.AsNoTracking()
                on new { pm.Project, DueMonth = pm.MonthNo } equals new { ms0.Project, ms0.DueMonth } into ms1
            from ms in ms1.DefaultIfEmpty()
            join tr0 in db.RsQryJobMonthTransfersTotal.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { tr0.Project, tr0.Month } into tr1
            from tr in tr1.DefaultIfEmpty()
            join iv0 in db.RsQryJobMonthInvoices.AsNoTracking()
                on new { ProjectParent = pm.Project, Month = pm.MonthNo } equals new { iv0.ProjectParent, iv0.Month } into iv1
            from iv in iv1.DefaultIfEmpty()
            join ps0 in db.RsQryJobMonthPortfolioSales.AsNoTracking()
                on new { PlanPortfolio = pm.Project, Month = pm.MonthNo } equals new { ps0.PlanPortfolio, ps0.Month } into ps1
            from ps in ps1.DefaultIfEmpty()
            join tp0 in db.RsQryJobMonthTotProfile.AsNoTracking()
                on pm.Project equals tp0.Project into tp1
            from tp in tp1.DefaultIfEmpty()
            select new RsProjectMonth2Table
            {
                Project = pm.Project,
                MonthNo = pm.MonthNo,
                CostProfile = pm.CostProfile,
                SubContracts = sc != null ? sc.Total ?? 0m : 0m,
                Animals = sc != null ? sc.Animals ?? 0m : 0m,
                NonAnimal = sc != null ? sc.Other ?? 0m : 0m,
                TimeCosts = tm != null ? tm.SumOfCost ?? 0d : 0d,
                TransferCosts = tr != null ? (double?)tr.SumOfTransferCost ?? 0d : 0d,
                TotalCost = (sc != null ? sc.Total ?? 0m : 0m)
                    + (decimal)(tm != null ? tm.SumOfCost ?? 0d : 0d)
                    + (tr != null ? tr.SumOfTransferCost ?? 0m : 0m),
                Invoices = iv != null ? iv.SumOfAmount1 ?? 0m : 0m,
                Coiw = tm != null ? tm.WorkCost ?? 0m : 0m,
                SumOfCostProfile = tp != null ? tp.SumOfCostProfile : null,
                PortSales = ps != null ? (double?)ps.Fee ?? 0d : 0d,
                MstoneDue = ms != null ? ms.MstoneDue : null,
                DueDone = ms != null ? ms.DueDone : null,
                OnTime = ms != null ? ms.OnTime : null,
                TotalHours = tm != null ? tm.SumOfHours ?? 0d : 0d,
                PayCosts = tm != null ? (double?)tm.SumOfPayRate ?? 0d : 0d
            })
            .ToListAsync(cancellationToken);

        await db.RsProjectMonth2.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
