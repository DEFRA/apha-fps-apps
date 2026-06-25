using System.Globalization;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class TlkpYearLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 16;

    public override string Name => "tlkpyear";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Ensure retries do not fail if a prior attempt inserted this year already.
        await context.MaDstTlkpYear
            .Where(x => x.Year == year)
            .ExecuteDeleteAsync(cancellationToken);

        var monthValues = await context.MaSrcTblDbVariable
            .AsNoTracking()
            .Where(v => v.DbVarName == "month")
            .Select(v => v.DbVarValue)
            .ToListAsync(cancellationToken);

        var latestMonths = monthValues
            .Select(value => new MaDstTlkpYear
            {
                LatestMonthReleased = value == null
                    ? null
                    : int.Parse(value, CultureInfo.InvariantCulture)
            }.LatestMonthReleased)
            .Distinct()
            .ToList();

        if (latestMonths.Count == 0)
        {
            return 0;
        }

        if (latestMonths.Count > 1)
        {
            throw new InvalidOperationException(
                $"Seq 16 TlkpYear: Expected a single 'month' value, but found {latestMonths.Count} distinct values.");
        }

        await context.MaDstTlkpYear.AddAsync(new MaDstTlkpYear
        {
            Year = year,
            LatestMonthReleased = latestMonths[0]
        }, cancellationToken);

        return await context.SaveChangesAsync(cancellationToken);
    }
}

