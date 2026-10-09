using System.Globalization;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class TlkpYearLoader : MabArchiveExecutionLoaderBase
{
    public TlkpYearLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 16;

    public override string Name => "tlkpyear";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Ensure retries do not fail if a prior attempt inserted this year already.
        await context.MaDstTlkpYear
            .Where(x => x.Year == year)
            .ExecuteDeleteAsync(cancellationToken);

        // Legacy source: tblDB_Variables 'month', matched case-insensitively (stored as 'Month').
        var values = await context.MaSrcTblDbVariable
            .AsNoTracking()
            .Where(v => v.DbVarName.ToLower() == "month")
            .Select(v => v.DbVarValue)
            .ToListAsync(cancellationToken);

        if (values.Count != 1)
        {
            throw new InvalidOperationException(
                $"Seq 16 TlkpYear: Expected exactly one 'Month' row in fps.tbldb_variables " +
                $"while loading mabarchive.tlkpyear for FPS year {year}, " +
                $"but found {values.Count}.");
        }

        var rawValue = values[0];

        if (!int.TryParse(rawValue?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || month > 12)
        {
            throw new InvalidOperationException(
                $"Seq 16 TlkpYear: Invalid 'Month' value '{rawValue ?? "<null>"}' in " +
                $"fps.tbldb_variables while loading mabarchive.tlkpyear " +
                $"for FPS year {year}. Expected an integer between 0 and 12.");
        }

        context.MaDstTlkpYear.Add(new MaDstTlkpYear
        {
            Year = year,
            LatestMonthReleased = month
        });

        return await context.SaveChangesAsync(cancellationToken);
    }
}
