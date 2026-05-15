using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblAnimalReqLinqLoader : MabArchiveLinqLoaderBase
{
    public override int Sequence => 11;

    public override string Name => "my_tblanimalreq";

    protected override async Task<int> LoadWithLinqAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTblAnimalReq
            .AsNoTracking()
            .Where(a => a.FpsYear == year)
            .Select(a => new MaDstMyTblAnimalReq
            {
                Year = year,
                JobCode = a.JobCode,
                AnimalType = a.AnimalType,
                NumberOfDays = a.NumberOfDays,
                NumberOfAnimals = a.NumberOfAnimals
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblAnimalReq.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}
