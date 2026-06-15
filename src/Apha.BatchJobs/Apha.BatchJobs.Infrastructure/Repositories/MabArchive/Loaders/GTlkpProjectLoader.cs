using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class GTlkpProjectLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 2;

    public override string Name => "g_tlkpproject";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTlkpProject
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .GroupBy(t => new
            {
                t.ParentProject,
                t.ProjectTitle,
                t.CostBookNo,
                t.Disease,
                t.Contract,
                t.ShortTitle,
                t.ProjectStatus
            })
            .Select(g => new MaDstGTlkpProject
            {
                ParentProject = g.Key.ParentProject,
                ProjectTitle = g.Key.ProjectTitle,
                CostBookNo = g.Key.CostBookNo,
                Disease = g.Key.Disease,
                Contract = g.Key.Contract,
                ShortTitle = g.Key.ShortTitle,
                ProjectStatus = g.Key.ProjectStatus
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstGTlkpProject.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

