using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.BulkRates.Repositories;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>PostgreSQL-backed test: <see cref="BulkRatesRepository.WriteJobQueueLogAsync"/>'s raw-SQL insert resolves and writes fpsyear.</summary>
[Trait("Category", "Integration")]
public sealed class BulkRatesRepositoryWriteJobQueueLogAsyncIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=LOCAL_DB_PASSWORD;SSL Mode=Disable";
    private readonly string _connectionString;
    private string? _skipReason;

    public BulkRatesRepositoryWriteJobQueueLogAsyncIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private BatchJobsDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<BatchJobsDbContext>().UseNpgsql(_connectionString).Options);

    private sealed class TestDbContextFactory(string connectionString) : IDbContextFactory<BatchJobsDbContext>
    {
        public BatchJobsDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<BatchJobsDbContext>().UseNpgsql(connectionString).Options);
    }

    private BulkRatesRepository CreateRepository() =>
        new(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance);

    private async Task<Guid> SeedJobQueueRowAsync(BatchJobsDbContext context, int? fpsYear)
    {
        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.BulkTestRatesUpdate}")
            .SingleAsync();
        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Running'")
            .SingleAsync();

        var jobQueueId = Guid.NewGuid();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear)
            VALUES
                ({jobQueueId}, {Guid.NewGuid()}, {jobId}, {statusId}, 'write-job-queue-log-test', NOW(), NOW(), {fpsYear});");

        return jobQueueId;
    }

    [SkippableFact]
    public async Task WriteJobQueueLogAsync_WhenParentHasFpsYear_WritesParentFpsYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        var jobQueueId = await SeedJobQueueRowAsync(context, fpsYear: 2025);

        try
        {
            await CreateRepository().WriteJobQueueLogAsync(jobQueueId, "test note", "test-actor");

            var logFpsYear = await context.Database
                .SqlQuery<int>($@"
                    SELECT fpsyear AS ""Value"" FROM fps.job_queue_log
                    WHERE jobqueueid = {jobQueueId} ORDER BY logtime DESC LIMIT 1")
                .SingleAsync();

            Assert.Equal(2025, logFpsYear);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
            await context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    [SkippableFact]
    public async Task WriteJobQueueLogAsync_WhenParentFpsYearIsNull_DerivesFpsYearFromLogTime()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        var jobQueueId = await SeedJobQueueRowAsync(context, fpsYear: null);
        var utcNow = DateTime.UtcNow;
        var expectedFpsYear = utcNow.Month >= 4 ? utcNow.Year : utcNow.Year - 1;

        try
        {
            await CreateRepository().WriteJobQueueLogAsync(jobQueueId, "test note", "test-actor");

            var logFpsYear = await context.Database
                .SqlQuery<int>($@"
                    SELECT fpsyear AS ""Value"" FROM fps.job_queue_log
                    WHERE jobqueueid = {jobQueueId} ORDER BY logtime DESC LIMIT 1")
                .SingleAsync();

            Assert.Equal(expectedFpsYear, logFpsYear);
        }
        finally
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
            await context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }
}
