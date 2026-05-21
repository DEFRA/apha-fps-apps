using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjSubcontractLoader : MabArchiveLinqLoaderBase
{
    public override int Sequence => 8;

    public override string Name => "my_proj_subcontract";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcProjSubContract
            .AsNoTracking()
            .Where(s => s.FpsYear == year)
            .Select(s => new
            {
                SubContCounter = s.SubContCounter,
                Project = s.Project,
                TestJob = s.TestJob,
                Month = s.Month,
                Amount = s.Amount,
                WorkGroup = s.WorkGroup,
                AcctCode = s.AcctCode,
                Supplier = s.Supplier,
                Description = s.Description,
                SupplierNumber = s.SupplierNumber,
                DailyRate = s.DailyRate,
                AnimalDays = s.AnimalDays
            })
            .ToListAsync(cancellationToken);

        var rows = sourceRows
            .Select(s => new MaDstMyProjSubContract
            {
                Year = year,
                SubContCounter = s.SubContCounter,
                Project = s.Project,
                TestJob = s.TestJob,
                Month = s.Month,
                Amount = s.Amount,
                WorkGroup = s.WorkGroup,
                AcctCode = s.AcctCode,
                Supplier = s.Supplier,
                Description = s.Description,
                SupplierNumber = s.SupplierNumber,
                DailyRate = s.DailyRate,
                AnimalDays = s.AnimalDays
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyProjSubContract.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}


