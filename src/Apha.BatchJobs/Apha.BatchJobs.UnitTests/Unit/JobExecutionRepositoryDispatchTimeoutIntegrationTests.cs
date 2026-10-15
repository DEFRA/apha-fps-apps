using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed tests for <see cref="JobExecutionRepository.FailStaleDispatchedExecutionsAsync"/>.
/// Each test seeds a throwaway job_master entry and its rows inside one transaction that is
/// always rolled back, so nothing is left behind and no real job's rows are touched.
/// </summary>
[Trait("Category", "Integration")]
public sealed class JobExecutionRepositoryDispatchTimeoutIntegrationTests : IAsyncLifetime
{
    private const int MinimumMinutes = 75;
    private readonly string _connectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString") ?? string.Empty;
    private string? _skipReason;

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            if (!await context.Database.CanConnectAsync())
                _skipReason = "Integration DB unavailable.";
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task FailsOnlyStaleUnlockedRowsInThePickupStatus_AndAppliesTheMinimum()
    {
        Skip.IfNot(string.IsNullOrWhiteSpace(_skipReason), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await using var tx = await context.Database.BeginTransactionAsync();

        // timetolive 1 minute: the 75-minute minimum must still protect the 30-minute-old row.
        var jobName = await SeedJobAsync(context, timeToLiveMinutes: 1);
        var stale = await SeedRowAsync(context, jobName, JobStatus.Approved, approvedMinutesAgo: 200);
        var recent = await SeedRowAsync(context, jobName, JobStatus.Approved, approvedMinutesAgo: 30);
        var staleButLocked = await SeedRowAsync(context, jobName, JobStatus.Approved, approvedMinutesAgo: 200);
        var running = await SeedRowAsync(context, jobName, JobStatus.Running, approvedMinutesAgo: 200);
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, jobqueueid, is_active)
            VALUES (NOW(), NOW() + INTERVAL '5 minutes', {jobName}, {staleButLocked}, TRUE);");

        var repository = new JobExecutionRepository(context, NullLogger<JobExecutionRepository>.Instance);
        var failed = await repository.FailStaleDispatchedExecutionsAsync(
            jobName, JobStatus.Approved, DispatchClock.ApprovedAt, MinimumMinutes, "user message", "diagnostic note");

        var row = Assert.Single(failed);
        Assert.Equal(stale, row.JobQueueId);
        Assert.Equal(MinimumMinutes, row.TimeToLiveMinutes);
        Assert.Equal("Failed", await StatusOfAsync(context, stale));
        Assert.Equal("user message", await ErrorMessageOfAsync(context, stale));
        Assert.Equal("Approved", await StatusOfAsync(context, recent));
        Assert.Equal("Approved", await StatusOfAsync(context, staleButLocked));
        Assert.Equal("Running", await StatusOfAsync(context, running));

        var logNotes = await context.Database
            .SqlQuery<string>($@"SELECT note AS ""Value"" FROM fps.job_queue_log WHERE jobqueueid = {stale}")
            .ToListAsync();
        Assert.Equal(["diagnostic note"], logNotes);

        await tx.RollbackAsync();
    }

    [SkippableFact]
    public async Task UsesTimeToLiveWhenAboveTheMinimum_AndRequestedAtForInitiatedJobs()
    {
        Skip.IfNot(string.IsNullOrWhiteSpace(_skipReason), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await using var tx = await context.Database.BeginTransactionAsync();

        var jobName = await SeedJobAsync(context, timeToLiveMinutes: 300);
        var withinTtl = await SeedRowAsync(context, jobName, JobStatus.Initiated, requestedMinutesAgo: 200);
        var pastTtl = await SeedRowAsync(context, jobName, JobStatus.Initiated, requestedMinutesAgo: 400);

        var repository = new JobExecutionRepository(context, NullLogger<JobExecutionRepository>.Instance);
        var failed = await repository.FailStaleDispatchedExecutionsAsync(
            jobName, JobStatus.Initiated, DispatchClock.RequestedAt, MinimumMinutes, "user message", "diagnostic note");

        var row = Assert.Single(failed);
        Assert.Equal(pastTtl, row.JobQueueId);
        Assert.Equal(300, row.TimeToLiveMinutes);
        Assert.Equal("Initiated", await StatusOfAsync(context, withinTtl));

        await tx.RollbackAsync();
    }

    private static async Task<string> SeedJobAsync(BatchJobsDbContext context, int timeToLiveMinutes)
    {
        var jobName = $"dispatch-timeout-test-{Guid.NewGuid():N}";
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_master (jobname, frequency, note, timetolive, created_at, updated_at)
            VALUES ({jobName}, 'Manual', 'integration test', {timeToLiveMinutes}, NOW(), NOW());");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_status (jobid, status)
            SELECT m.jobid, s.status
            FROM fps.job_master m
            CROSS JOIN (VALUES ('Initiated'), ('Approved'), ('Running'), ('Failed')) AS s(status)
            WHERE m.jobname = {jobName};");
        return jobName;
    }

    private static async Task<Guid> SeedRowAsync(
        BatchJobsDbContext context, string jobName, JobStatus status, int? approvedMinutesAgo = null, int? requestedMinutesAgo = null)
    {
        var jobQueueId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow.AddMinutes(-(requestedMinutesAgo ?? 500));
        DateTime? approvedAt = approvedMinutesAgo is null ? null : DateTime.UtcNow.AddMinutes(-approvedMinutesAgo.Value);
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, approved_at_utc, fpsyear)
            SELECT {jobQueueId}, {Guid.NewGuid()}, m.jobid, s.statusid, 'dispatch-timeout-test', {requestedAt}, {approvedAt}, 2026
            FROM fps.job_master m JOIN fps.job_status s ON s.jobid = m.jobid
            WHERE m.jobname = {jobName} AND s.status = {status.ToString()};");
        return jobQueueId;
    }

    private static Task<string> StatusOfAsync(BatchJobsDbContext context, Guid jobQueueId) =>
        context.Database.SqlQuery<string>($@"
            SELECT s.status AS ""Value"" FROM fps.job_queue q JOIN fps.job_status s ON s.statusid = q.statusid
            WHERE q.jobqueueid = {jobQueueId}").SingleAsync();

    private static Task<string?> ErrorMessageOfAsync(BatchJobsDbContext context, Guid jobQueueId) =>
        context.Database.SqlQuery<string?>($@"
            SELECT errormessage AS ""Value"" FROM fps.job_queue WHERE jobqueueid = {jobQueueId}").SingleAsync();

    private BatchJobsDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<BatchJobsDbContext>().UseNpgsql(_connectionString).Options);
}
