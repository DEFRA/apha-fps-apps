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
/// PostgreSQL-backed regression test for <see cref="JobExecutionRepository.CreateExecutionRecordAsync"/>'s
/// Approved/Initiated → Running transition of a pre-created queue row. The incoming record's
/// <c>FpsYear</c> is the target/planned year read from job parameters, not the row's current year, and
/// must never be written onto <c>fps.job_queue.fpsyear</c> on pickup.
/// </summary>
[Trait("Category", "Integration")]
public sealed class JobExecutionRepositoryFpsYearIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndCatalogAvailable;

    public JobExecutionRepositoryFpsYearIntegrationTests()
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

            _yearEndCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.YearEndDataSetup}
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
    public async Task CreateExecutionRecordAsync_OnApprovedToRunningTransition_PreservesExistingFpsYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndCatalogAvailable,
            $"job_status seed for '{BatchJobNames.YearEndDataSetup}' Approved/Running is not yet provisioned on this database.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        const int currentFpsYear = 2025;
        const int targetFpsYear = 2026;

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
                .SingleAsync();

            var approvedStatusId = await context.Database
                .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear, target_fpsyear)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {approvedStatusId}, 'fpsyear-preservation-test', NOW(), NOW(), {currentFpsYear}, {targetFpsYear});");
        }

        try
        {
            var repository = CreateRepository();

            // Mirrors what JobOrchestrator sends on pickup: FpsYear is the target/planned year
            // extracted from job parameters, not this row's current year.
            var record = new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = BatchJobNames.YearEndDataSetup,
                JobExecutionId = jobExecutionId,
                JobQueueId = jobQueueId,
                UserId = "fpsyear-preservation-test-worker",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow,
                FpsYear = targetFpsYear
            };

            await repository.CreateExecutionRecordAsync(record);

            await using var assertContext = CreateDbContext();
            var persisted = await assertContext.Database
                .SqlQuery<PersistedYears>($@"
                    SELECT fpsyear AS ""FpsYear"", target_fpsyear AS ""TargetFpsYear""
                    FROM fps.job_queue
                    WHERE jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(currentFpsYear, persisted.FpsYear);
            Assert.Equal(targetFpsYear, persisted.TargetFpsYear);
        }
        finally
        {
            await using var context = CreateDbContext();
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    [SkippableFact]
    public async Task CreateExecutionRecordAsync_OnApprovedToRunningTransition_PersistsTargetFpsYearWhenPreviouslyNull()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndCatalogAvailable,
            $"job_status seed for '{BatchJobNames.YearEndDataSetup}' Approved/Running is not yet provisioned on this database.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        const int currentFpsYear = 2025;
        const int targetFpsYear = 2026;

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
                .SingleAsync();

            var approvedStatusId = await context.Database
                .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
                .SingleAsync();

            // target_fpsyear starts NULL here — the FPS API doesn't persist it at Initiate time;
            // this test proves the Worker's own Approved → Running transition fills the gap.
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {approvedStatusId}, 'target-fpsyear-persist-test', NOW(), NOW(), {currentFpsYear});");
        }

        try
        {
            var repository = CreateRepository();

            var record = new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = BatchJobNames.YearEndDataSetup,
                JobExecutionId = jobExecutionId,
                JobQueueId = jobQueueId,
                UserId = "target-fpsyear-persist-test-worker",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow,
                FpsYear = targetFpsYear,
                TargetFpsYear = targetFpsYear
            };

            await repository.CreateExecutionRecordAsync(record);

            await using var assertContext = CreateDbContext();
            var persisted = await assertContext.Database
                .SqlQuery<PersistedYears>($@"
                    SELECT fpsyear AS ""FpsYear"", target_fpsyear AS ""TargetFpsYear""
                    FROM fps.job_queue
                    WHERE jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(currentFpsYear, persisted.FpsYear);
            Assert.Equal(targetFpsYear, persisted.TargetFpsYear);
        }
        finally
        {
            await using var context = CreateDbContext();
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    [SkippableFact]
    public async Task CreateExecutionRecordAsync_WhenRecordTargetFpsYearIsNull_NeverBlanksExistingValue()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndCatalogAvailable,
            $"job_status seed for '{BatchJobNames.YearEndDataSetup}' Approved/Running is not yet provisioned on this database.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();
        const int currentFpsYear = 2025;
        const int targetFpsYear = 2026;

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
                .SingleAsync();

            var approvedStatusId = await context.Database
                .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime, fpsyear, target_fpsyear)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {approvedStatusId}, 'target-fpsyear-safe-test', NOW(), NOW(), {currentFpsYear}, {targetFpsYear});");
        }

        try
        {
            var repository = CreateRepository();

            // No parametersJson target year for this run — record.TargetFpsYear left unset (null).
            var record = new JobExecutionRecord
            {
                ExecutionId = 0,
                JobName = BatchJobNames.YearEndDataSetup,
                JobExecutionId = jobExecutionId,
                JobQueueId = jobQueueId,
                UserId = "target-fpsyear-safe-test-worker",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Running,
                StartedAt = DateTime.UtcNow
            };

            await repository.CreateExecutionRecordAsync(record);

            await using var assertContext = CreateDbContext();
            var persisted = await assertContext.Database
                .SqlQuery<PersistedYears>($@"
                    SELECT fpsyear AS ""FpsYear"", target_fpsyear AS ""TargetFpsYear""
                    FROM fps.job_queue
                    WHERE jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(targetFpsYear, persisted.TargetFpsYear);
        }
        finally
        {
            await using var context = CreateDbContext();
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    private sealed record PersistedYears(int FpsYear, int? TargetFpsYear);

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
