using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblContractLoader : MabArchiveLinqLoaderBase
{
    public override int Sequence => 12;

    public override string Name => "my_tblcontract";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTblContract
            .AsNoTracking()
            .Where(c => c.FpsYear == year)
            .Select(c => new MaDstMyTblContract
            {
                Year = year,
                ContractNo = c.ContractNo,
                Category = c.Category,
                Manager = c.Manager,
                Customer = c.Customer,
                Title = c.Title,
                RegisteredDate = c.RegisteredDate,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ContractDoc = c.ContractDoc,
                Duration = c.Duration
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblContract.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}


