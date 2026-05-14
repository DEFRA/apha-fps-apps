using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCasework";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rawRows = await db.RsQryProjectMonthCw
            .AsNoTracking()
            .Select(x => new
            {
                Project = x.Project,
                MonthNo = x.MonthNo,
                CwDebit = x.CwDebit,
                CwCredit = x.CwCredit
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(x => new RsProjectMonthCaseworkTable
        {
            Project = x.Project,
            MonthNo = x.MonthNo,
            CwDebit = x.CwDebit.HasValue ? (double?)x.CwDebit.Value : null,
            CwCredit = x.CwCredit.HasValue ? (double?)x.CwCredit.Value : null
        }).ToList();

        await db.RsProjectMonthCasework.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
