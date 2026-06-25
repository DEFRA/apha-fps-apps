using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class GTlkpProjectLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 2;

    public override string Name => "g_tlkpproject";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTlkpProject
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new
            {
                t.ParentProject,
                t.ProjectTitle,
                t.CostBookNo,
                t.Disease,
                t.Contract,
                t.ShortTitle,
                t.ProjectStatus
            })
            .ToListAsync(cancellationToken);

        if (sourceRows.Count == 0)
        {
            return 0;
        }

        // g_tlkpproject is global and keyed by parentproject only.
        // Keep one source row per key and skip keys already present to make retries idempotent.
        var dedupedByParentProject = sourceRows
            .GroupBy(t => t.ParentProject)
            .Select(g => g.First())
            .ToList();

        var candidateKeys = dedupedByParentProject
            .Select(t => t.ParentProject)
            .ToList();

        var existingKeys = await context.MaDstGTlkpProject
            .AsNoTracking()
            .Where(t => candidateKeys.Contains(t.ParentProject))
            .Select(t => t.ParentProject)
            .ToListAsync(cancellationToken);

        var existingKeySet = existingKeys.ToHashSet(StringComparer.Ordinal);

        var rowsToInsert = dedupedByParentProject
            .Where(t => !existingKeySet.Contains(t.ParentProject))
            .Select(t => new MaDstGTlkpProject
            {
                ParentProject = t.ParentProject,
                ProjectTitle = t.ProjectTitle,
                CostBookNo = t.CostBookNo,
                Disease = t.Disease,
                Contract = t.Contract,
                ShortTitle = t.ShortTitle,
                ProjectStatus = t.ProjectStatus
            })
            .ToList();

        if (rowsToInsert.Count == 0)
        {
            return 0;
        }

        await context.MaDstGTlkpProject.AddRangeAsync(rowsToInsert, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

