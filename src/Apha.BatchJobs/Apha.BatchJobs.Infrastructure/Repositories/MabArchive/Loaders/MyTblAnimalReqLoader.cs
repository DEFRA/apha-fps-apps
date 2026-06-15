using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblAnimalReqLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 11;

    public override string Name => "my_tblanimalreq";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Keep source insertion order so explicit ar_counter values match SQL loader behavior.
        var sourceRows = await context.MaSrcTblAnimalReq
            .AsNoTracking()
            .Where(a => a.FpsYear == year)
            .OrderBy(a => a.IndCounter)
            .ToListAsync(cancellationToken);

        if (sourceRows.Count == 0)
        {
            return 0;
        }

        var firstCounter = await GetNextArCounterAsync(context, cancellationToken);

        var rows = sourceRows
            .Select((a, index) => new MaDstMyTblAnimalReq
            {
                Year = year,
                JobCode = a.JobCode,
                AnimalType = a.AnimalType,
                NumberOfDays = a.NumberOfDays,
                NumberOfAnimals = a.NumberOfAnimals,
                ArCounter = firstCounter + index
            })
            .ToList();

        await context.MaDstMyTblAnimalReq.AddRangeAsync(rows, cancellationToken);
        var affectedRows = await context.SaveChangesAsync(cancellationToken);

        var lastCounter = firstCounter + rows.Count - 1;
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT setval('mabarchive.my_tblanimalreq_ar_counter_seq', {lastCounter}, true)",
            cancellationToken);

        return affectedRows;
    }

    private static async Task<int> GetNextArCounterAsync(BatchJobsDbContext context, CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var closeAfter = connection.State != System.Data.ConnectionState.Open;

        if (closeAfter)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT last_value, is_called FROM mabarchive.my_tblanimalreq_ar_counter_seq";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("Could not read sequence state for my_tblanimalreq ar_counter.");
            }

            var lastValue = reader.GetInt64(0);
            var isCalled = reader.GetBoolean(1);
            var nextValue = isCalled ? lastValue + 1 : lastValue;
            return checked((int)nextValue);
        }
        finally
        {
            if (closeAfter)
            {
                await connection.CloseAsync();
            }
        }
    }
}



