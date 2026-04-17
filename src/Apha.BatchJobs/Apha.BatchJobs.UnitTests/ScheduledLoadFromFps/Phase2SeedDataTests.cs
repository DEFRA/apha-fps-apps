using Npgsql;

namespace Apha.BatchJobs.UnitTests.ScheduledLoadFromFps;

/// <summary>
/// Phase 2 seed-data verification tests.
/// These tests use the local integration database and skip gracefully when unavailable.
/// </summary>
public sealed class Phase2SeedDataTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public Phase2SeedDataTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await conn.CloseAsync();
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Seed_JobMaster_Contains_ScheduledLoadFromFps()
    {
        Assert.True(CanRunIntegrationTests(), _skipReason);

        var count = await ScalarIntAsync(@"
SELECT COUNT(*)
FROM fps.job_master
WHERE jobname = 'ScheduledLoadFromFps'
");

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Seed_JobStatus_ContainsFiveStatuses_ForScheduledLoadFromFps()
    {
        Assert.True(CanRunIntegrationTests(), _skipReason);

        var count = await ScalarIntAsync(@"
SELECT COUNT(DISTINCT js.status)
FROM fps.job_status js
INNER JOIN fps.job_master jm ON jm.jobid = js.jobid
WHERE jm.jobname = 'ScheduledLoadFromFps'
  AND js.status IN ('Queued', 'Running', 'Completed', 'Failed', 'Cancelled')
");

        Assert.Equal(5, count);
    }

    [Fact]
    public async Task Seed_SourceTables_HaveSixRows_ForFixtureProjects()
    {
        Assert.True(CanRunIntegrationTests(), _skipReason);

        var totalsCount = await ScalarIntAsync(@"
SELECT COUNT(*)
FROM fps.fpsyeartotals
WHERE parentproject LIKE 'P00%\_%' ESCAPE '\\'
");

        var projectCount = await ScalarIntAsync(@"
SELECT COUNT(*)
FROM fps.tlkpproject
WHERE parentproject LIKE 'P00%\_%' ESCAPE '\\'
");

        Assert.Equal(6, totalsCount);
        Assert.Equal(6, projectCount);
    }

    [Fact]
    public async Task Seed_ArchiveTables_ArePresentForBaselineYear()
    {
        Assert.True(CanRunIntegrationTests(), _skipReason);

        var year = 2025;
        var archiveTotals = await ScalarIntAsync("SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year = @year", new NpgsqlParameter("year", year));
        var archiveProjectAll = await ScalarIntAsync("SELECT COUNT(*) FROM mabarchive.my_tlkpproject_all WHERE year = @year", new NpgsqlParameter("year", year));

        Assert.True(archiveTotals > 0);
        Assert.True(archiveProjectAll > 0);
    }

    [Fact]
    public async Task Seed_BaselineValidationRecords_Exist()
    {
        Assert.True(CanRunIntegrationTests(), _skipReason);

        var count = await ScalarIntAsync(@"
SELECT COUNT(*)
FROM fps.scheduled_load_validation_result
WHERE assertion_code IN ('BASELINE_001', 'BASELINE_002', 'BASELINE_003')
");

        Assert.Equal(3, count);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private async Task<int> ScalarIntAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        if (parameters.Length > 0)
        {
            cmd.Parameters.AddRange(parameters);
        }

        var value = await cmd.ExecuteScalarAsync();
        return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }
}
