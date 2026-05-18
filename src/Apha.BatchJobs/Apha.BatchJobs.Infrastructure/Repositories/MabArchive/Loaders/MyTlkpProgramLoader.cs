using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpProgramDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 1;

    public override string Name => "my_tlkpprogram";

    protected override async Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTlkpProgram
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new
            {
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToListAsync(cancellationToken);

        var rows = sourceRows
            .Select(p => new MaDstMyTlkpProgram
            {
                Year = year,
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        var inserted = 0;

        foreach (var row in rows)
        {
            await context.MaDstMyTlkpProgram.AddAsync(row, cancellationToken);
            inserted += await context.SaveChangesAsync(cancellationToken);
            context.Entry(row).State = EntityState.Detached;
        }

        return inserted;
    }
}


