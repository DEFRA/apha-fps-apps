using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyTlkpTestReqmtLoader : MabArchiveExecutionLoaderBase
{
    public MyTlkpTestReqmtLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 15;

    public override string Name => "my_tlkptestreqmt";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // PK is (year, testcode, buyer); skip rows where either key column is null/blank.
        var rows = await context.MaSrcTlkpTestReqmt
            .AsNoTracking()
            .Where(t => t.FpsYear == year && !string.IsNullOrWhiteSpace(t.TestCode) && !string.IsNullOrWhiteSpace(t.Buyer))
            .Select(t => new MaDstMyTlkpTestReqmt
            {
                Year = year,
                TestCode = t.TestCode!,
                Buyer = t.Buyer!,
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

