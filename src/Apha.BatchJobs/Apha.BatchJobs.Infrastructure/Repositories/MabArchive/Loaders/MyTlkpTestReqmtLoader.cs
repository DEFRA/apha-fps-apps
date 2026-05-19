using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpTestReqmtDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 15;

    public override string Name => "my_tlkptestreqmt";

    protected override async Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTlkpTestReqmt
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new MaDstMyTlkpTestReqmt
            {
                Year = year,
                TestCode = t.TestCode,
                Buyer = t.Buyer,
                UnitPrice = t.UnitPrice,
                NoRequired = t.NoRequired,
                ProjectBuyerCode = t.ProjectBuyerCode,
                TestBuyerCode = t.TestBuyerCode
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTlkpTestReqmt.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}
