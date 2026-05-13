using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqCreateProjectMonthSingleStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthSingle";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Two-step: fetch raw nullable values first (avoid COALESCE on PostgreSQL money columns),
        // then apply defaults in C#.
        var rawRows = await (
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
            select new
            {
                Project = pm.Project,
                MonthNo = pm.MonthNo,
                CostProfile = pm.CostProfile,
                ScTotal = sc.Total,
                ScAnimals = sc.Animals,
                ScOther = sc.Other,
                TmSumOfCost = tm.SumOfCost,
                TrSumOfTransferCost = tr.SumOfTransferCost,
                IvSumOfAmount1 = iv.SumOfAmount1,
                TmWorkCost = tm.WorkCost,
                TpSumOfCostProfile = tp.SumOfCostProfile,
                PsFee = ps.Fee,
                MsMstoneDue = ms.MstoneDue,
                MsDueDone = ms.DueDone,
                MsOnTime = ms.OnTime,
                TmSumOfHours = tm.SumOfHours,
                TmSumOfPayRate = tm.SumOfPayRate
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsProjectMonth2Table
        {
            Project = r.Project,
            MonthNo = r.MonthNo,
            CostProfile = r.CostProfile,
            SubContracts = r.ScTotal ?? 0m,
            Animals = r.ScAnimals ?? 0m,
            NonAnimal = r.ScOther ?? 0m,
            TimeCosts = r.TmSumOfCost ?? 0d,
            TransferCosts = (double?)(r.TrSumOfTransferCost) ?? 0d,
            TotalCost = (r.ScTotal ?? 0m)
                + (decimal)(r.TmSumOfCost ?? 0d)
                + (r.TrSumOfTransferCost ?? 0m),
            Invoices = r.IvSumOfAmount1 ?? 0m,
            Coiw = r.TmWorkCost ?? 0m,
            SumOfCostProfile = r.TpSumOfCostProfile,
            PortSales = (double?)(r.PsFee) ?? 0d,
            MstoneDue = r.MsMstoneDue,
            DueDone = r.MsDueDone,
            OnTime = r.MsOnTime,
            TotalHours = r.TmSumOfHours ?? 0d,
            PayCosts = (double?)(r.TmSumOfPayRate) ?? 0d
        }).ToList();

        await db.RsProjectMonth2.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
