using System.Globalization;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class TlkpYearLinqLoader : MabArchiveLinqLoaderBase
{
    public override int Sequence => 16;

    public override string Name => "tlkpyear";

    protected override async Task<int> LoadWithLinqAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var monthValues = await context.MaSrcTblDbVariable
            .AsNoTracking()
            .Where(v => v.DbVarName == "month")
            .Select(v => v.DbVarValue)
            .ToListAsync(cancellationToken);

        var rows = monthValues
            .Select(value => new MaDstTlkpYear
            {
                Year = year,
                LatestMonthReleased = value == null
                    ? null
                    : int.Parse(value, CultureInfo.InvariantCulture)
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstTlkpYear.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}
