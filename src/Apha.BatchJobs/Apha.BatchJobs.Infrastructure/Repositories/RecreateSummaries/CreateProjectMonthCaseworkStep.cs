using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCasework";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await db.RsQryProjectMonthCw
            .AsNoTracking()
            .Select(x => new RsProjectMonthCaseworkTable
            {
                Project = x.Project,
                MonthNo = x.MonthNo,
                CwDebit = (double?)x.CwDebit,
                CwCredit = (double?)x.CwCredit
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        await db.RsProjectMonthCasework.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
