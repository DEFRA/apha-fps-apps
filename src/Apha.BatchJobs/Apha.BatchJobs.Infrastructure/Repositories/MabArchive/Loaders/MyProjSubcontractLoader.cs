using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjSubcontractLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 8;

    public override string Name => "my_proj_subcontract";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO mabarchive.my_proj_subcontract
            (
                year,
                subcontcounter,
                project,
                testjob,
                month,
                amount,
                workgroup,
                acctcode,
                supplier,
                description,
                suppliernumber,
                dailyrate,
                animaldays
            )
            SELECT
                {year},
                p.subcontcounter,
                p.project,
                p.testjob,
                p.month,
                p.amount,
                p.workgroup,
                p.acctcode,
                p.supplier,
                p.description,
                p.suppliernumber,
                p.dailyrate,
                p.animaldays
            FROM fps.proj_subcontract p
            WHERE p.fpsyear = {year}
              AND p.month IS NOT NULL;
        ", cancellationToken);
    }
}



