using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCasework";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Strict SQL alignment: SELECT DISTINCT from qryProjectMonthCW, all fields, null handling.
        var rawRows = await db.RsQryProjectMonthCw
            .AsNoTracking()
            .Select(x => new
            {
                Project = x.Project,
                MonthNo = x.MonthNo,
                CwDebit = x.CwDebit ?? 0m,
                CwCredit = x.CwCredit ?? 0m
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(x => new RsProjectMonthCaseworkTable
        {
            Project = x.Project,
            MonthNo = x.MonthNo,
            CwDebit = (double)x.CwDebit,
            CwCredit = (double)x.CwCredit
        }).ToList();

        await db.RsProjectMonthCasework.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
