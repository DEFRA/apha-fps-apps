using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCasework";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Preserve legacy data-shape logic while making year explicit in the shared multi-year DB.
        return await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.projectmonthcasework (project, monthno, fpsyear, cwdebit, cwcredit)
            SELECT DISTINCT
                q.project,
                q.monthno,
                pm.fpsyear,
                COALESCE(q.cwdebit, 0),
                COALESCE(q.cwcredit, 0)
            FROM fps.qryprojectmonthcw q
            JOIN fps.projectmonth pm
              ON pm.project = q.project
             AND pm.monthno = q.monthno
             AND pm.fpsyear = {context.FpsYear};", cancellationToken);
    }
}
