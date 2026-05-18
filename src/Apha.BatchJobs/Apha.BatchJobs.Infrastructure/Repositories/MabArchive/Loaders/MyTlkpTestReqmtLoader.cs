using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpTestReqmtDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 15;

    public override string Name => "my_tlkptestreqmt";

    protected override async Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return await context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO mabarchive.my_tlkptestreqmt (
    year,
    testcode,
    buyer,
    unitprice,
    norequired,
    projectbuyercode,
    testbuyercode
)
SELECT
    {year},
    t.testcode,
    t.buyer,
    t.unitprice,
    t.norequired,
    t.projectbuyercode,
    t.testbuyercode
FROM fps.tlkptestreqmt AS t
WHERE t.fpsyear = {year};
", cancellationToken);
    }
}
