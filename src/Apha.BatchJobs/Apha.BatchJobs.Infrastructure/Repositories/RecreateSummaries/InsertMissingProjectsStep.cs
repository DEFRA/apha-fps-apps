using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class InsertMissingProjectsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "InsertMissingProjects";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var rowsAffected = 0;

        // RecreateSummaries runs multiple steps in one DbContext scope; clear any stale tracked entities
        // so this step can add projectmonth rows without tracker collisions.
        db.ChangeTracker.Clear();

        for (var month = 1; month <= 12; month++)
        {
            // Preserve legacy intent (insert missing project-month rows), but scope identity to execution year
            // in the shared multi-year database.
            var missingProjects = await (
                from p in db.RsTlkpProject.AsNoTracking()
                where p.FpsYear == context.FpsYear
                where !db.RsProjectMonth.AsNoTracking().Any(pm =>
                    pm.Project == p.ParentProject &&
                    pm.MonthNo == month &&
                    pm.FpsYear == context.FpsYear)
                select p.ParentProject)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (missingProjects.Count == 0)
            {
                continue;
            }

            var inserts = missingProjects.Select(parentProject => new RsProjectMonthTable
            {
                Project = parentProject,
                MonthNo = month,
                FpsYear = context.FpsYear
            })
            .GroupBy(x => new { x.Project, x.MonthNo, x.FpsYear })
            .Select(g => g.First())
            .ToList();

            await db.RsProjectMonth.AddRangeAsync(inserts, cancellationToken);
            rowsAffected += await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }

        return rowsAffected;
    }
}
