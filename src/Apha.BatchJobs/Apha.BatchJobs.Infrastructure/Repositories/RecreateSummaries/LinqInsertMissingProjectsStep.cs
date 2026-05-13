using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class LinqInsertMissingProjectsStep : LinqRecreateSummariesExecutionStepBase
{
    public override string StepName => "InsertMissingProjects";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var rowsAffected = 0;

        for (var month = 1; month <= 12; month++)
        {
            var missingProjects = await (
                from p in db.RsTlkpProject.AsNoTracking()
                where !db.RsProjectMonth.AsNoTracking().Any(pm => pm.Project == p.ParentProject && pm.MonthNo == month)
                orderby p.ParentProject
                select p.ParentProject)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (missingProjects.Count == 0)
            {
                continue;
            }

            var inserts = missingProjects.Select(project => new RsProjectMonthTable
            {
                Project = project,
                MonthNo = month
            });

            await db.RsProjectMonth.AddRangeAsync(inserts, cancellationToken);
            rowsAffected += await db.SaveChangesAsync(cancellationToken);
        }

        return rowsAffected;
    }
}
