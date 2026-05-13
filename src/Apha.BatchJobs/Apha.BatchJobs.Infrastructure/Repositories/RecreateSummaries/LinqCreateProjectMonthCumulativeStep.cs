using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqCreateProjectMonthCumulativeStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCumulative";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await (
            from tp in db.RsTblPeriod.AsNoTracking()
            join tpm in db.RsTblkPeriodMonth.AsNoTracking()
                on tp.PeriodName equals tpm.PeriodName
            join pm2 in db.RsProjectMonth2.AsNoTracking()
                on tpm.MonthNo equals pm2.MonthNo
            join pmcw in db.RsProjectMonthCasework.AsNoTracking()
                on new { pm2.Project, pm2.MonthNo } equals new { pmcw.Project, pmcw.MonthNo }
            group new { pm2, pmcw } by new { tp.EndPeriod, tp.PeriodName, pm2.Project, pm2.SumOfCostProfile }
            into g
            select new RsProjectMonth3Table
            {
                EndPeriod = g.Key.EndPeriod,
                PeriodName = g.Key.PeriodName,
                Project = g.Key.Project,
                CumCost = g.Sum(x => x.pm2.TotalCost) ?? 0m,
                CumInvoices = g.Sum(x => x.pm2.Invoices) ?? 0m,
                CumCoiw = g.Sum(x => x.pm2.Coiw) ?? 0m,
                CumPortSales = (decimal?)g.Sum(x => x.pm2.PortSales ?? 0d),
                CumProfile = g.Sum(x => x.pm2.CostProfile) ?? 0m,
                SumOfCostProfile = g.Key.SumOfCostProfile,
                SumOfMstoneDue = g.Sum(x => x.pm2.MstoneDue ?? 0d),
                SumOfDueDone = g.Sum(x => x.pm2.DueDone ?? 0d),
                SumOfOnTime = g.Sum(x => x.pm2.OnTime ?? 0d),
                CumCwDebit = (decimal?)g.Sum(x => x.pmcw.CwDebit ?? 0d),
                CumCwCredit = (decimal?)g.Sum(x => x.pmcw.CwCredit ?? 0d),
                CumTotalHours = g.Sum(x => x.pm2.TotalHours ?? 0d),
                CumSubContracts = g.Sum(x => (double)(x.pm2.SubContracts ?? 0m)),
                CumTestCosts = g.Sum(x => x.pm2.TransferCosts ?? 0d),
                CumPayCosts = g.Sum(x => x.pm2.PayCosts ?? 0d)
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        await db.RsProjectMonth3.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
