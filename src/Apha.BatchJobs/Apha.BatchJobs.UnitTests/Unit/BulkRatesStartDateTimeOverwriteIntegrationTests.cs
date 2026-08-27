using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration test proving the Running transition unconditionally overwrites
/// a producer-seeded StartDateTime with the actual execution start. This is the invariant the
/// Bulk Rates "populate StartDateTime at queue creation" design depends on — see
/// JobExecutionRepository.CreateExecutionRecordAsync's ExecuteUpdateAsync(SetProperty(StartDateTime, ...)).
/// </summary>
[Trait("Category", "Integration")]
public sealed class BulkRatesStartDateTimeOverwriteIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _bulkTestRatesCatalogAvailable;

    public BulkRatesStartDateTimeOverwriteIntegrationTests()
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
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            _bulkTestRatesCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.BulkTestRatesUpdate}
                      AND s.status IN ('Approved', 'Running')")
                .SingleAsync() >= 2;
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task CreateExecutionRecordAsync_WhenTransitioningToRunning_OverwritesProducerSeededStartDateTime()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _bulkTestRatesCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.BulkTestRatesUpdate}' + 'Approved'/'Running' is not yet provisioned on this database.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        // The producer-seeded value at queue creation — deliberately far in the past so it can
        // never be mistaken for the worker's own "now" if the overwrite silently fails to apply.
        var producerSeededStartDateTime = DateTime.UtcNow.AddDays(-3);
        var actualExecutionStart = DateTime.UtcNow;

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"
                    SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.BulkTestRatesUpdate}")
                .SingleAsync();

            var approvedStatusId = await context.Database
                .SqlQuery<int>($@"
                    SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {approvedStatusId}, 'integration-test-requester',
                     {producerSeededStartDateTime}, {producerSeededStartDateTime});");
        }

        try
        {
            var repository = CreateRepository();

            await repository.CreateExecutionRecordAsync(new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = BatchJobNames.BulkTestRatesUpdate,
                JobExecutionId = jobExecutionId,
                JobQueueId = jobQueueId,
                UserId = "integration-test-worker",
                JobType = JobType.DataLoad,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = actualExecutionStart
            });

            await using var verifyContext = CreateDbContext();
            var persistedStartDateTime = await verifyContext.Database
                .SqlQuery<DateTime>($@"
                    SELECT startdatetime AS ""Value"" FROM fps.job_queue WHERE jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.NotEqual(producerSeededStartDateTime, persistedStartDateTime);
            Assert.Equal(actualExecutionStart, persistedStartDateTime, TimeSpan.FromSeconds(1));
        }
        finally
        {
            await using var context = CreateDbContext();
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    private JobExecutionRepository CreateRepository() => new(CreateDbContext(), NullLogger<JobExecutionRepository>.Instance);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
