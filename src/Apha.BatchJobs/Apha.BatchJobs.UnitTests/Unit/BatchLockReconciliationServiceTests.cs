using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed tests for <see cref="BatchLockReconciliationService"/> (Phase 2 of orphan
/// lock reconciliation), using the real Phase 1 <see cref="BatchLockRepository"/> and
/// <see cref="JobExecutionRepository"/> against a live database rather than mocks — the same
/// end-to-end pattern as <c>BatchLockRepositoryTests</c>. See
/// batchjobs-job-lock-lease-heartbeat-worker-implementation-spec-2026-09-18.md.
/// </summary>
[Trait("Category", "Integration")]
public sealed class BatchLockReconciliationServiceTests : IAsyncLifetime
{
    private readonly string _connectionString;
    private string? _skipReason;

    public BatchLockReconciliationServiceTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? string.Empty;
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
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task ReconcileAsync_ExpiredLockWithRunningExecution_MarksFailedPreservesRequestedByAndDeletesLock()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        const string originalRequestedBy = "phase2-reconciliation-original-requester";
        var (jobQueueId, jobExecutionId) = await SeedJobQueueRowAsync(JobStatus.Running, originalRequestedBy);
        var expiredLock = await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var service = CreateService();

            await service.ReconcileAsync(expiredLock);

            await using var context = CreateDbContext();
            var persisted = await context.Database
                .SqlQuery<PersistedRow>($@"
                    SELECT s.status AS ""Status"", q.errormessage AS ""ErrorMessage"", q.requestedby AS ""RequestedBy""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(nameof(JobStatus.Failed), persisted.Status);
            Assert.Equal(BatchLockReconciliationService.ReconciledErrorMessage, persisted.ErrorMessage);
            // RequestedBy must survive exactly unchanged — reconciliation must never make itself
            // look like the original requester.
            Assert.Equal(originalRequestedBy, persisted.RequestedBy);

            var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
            Assert.Null(await repository.GetLockAsync(lockName));
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task ReconcileAsync_ExpiredLockWithTerminalExecution_LeavesStatusUnchangedButDeletesLock()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, jobExecutionId) = await SeedJobQueueRowAsync(JobStatus.Completed, "phase2-terminal-test-requester");
        var expiredLock = await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var service = CreateService();

            await service.ReconcileAsync(expiredLock);

            await using var context = CreateDbContext();
            var persistedStatus = await context.Database
                .SqlQuery<string>($@"
                    SELECT s.status AS ""Value""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(nameof(JobStatus.Completed), persistedStatus);

            var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
            Assert.Null(await repository.GetLockAsync(lockName));
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task ReconcileAsync_ExpiredLockWithNoMatchingExecution_DeletesLockAndLogsWarning()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        // No job_queue row ever seeded for this JobQueueId — the lock references nothing.
        var orphanJobQueueId = Guid.NewGuid();
        var expiredLock = await SeedExpiredLockRowAsync(lockName, orphanJobQueueId);

        try
        {
            var capturingLogger = new CapturingLogger<BatchLockReconciliationService>();
            var service = CreateService(capturingLogger);

            await service.ReconcileAsync(expiredLock);

            await using var context = CreateDbContext();
            var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
            Assert.Null(await repository.GetLockAsync(lockName));

            Assert.Contains(
                capturingLogger.Entries,
                e => e.Level == LogLevel.Warning && e.Message.Contains("no matching job_queue row", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            await CleanupAsync(lockName, orphanJobQueueId);
        }
    }

    [SkippableFact]
    public async Task ReconcileAsync_WhenLockNoLongerExpiredAtDeleteTime_LeavesLockIntact()
    {
        // Terminal execution deliberately used here — isolates the deletion-conditionality proof
        // from the separate mark-Failed behaviour already covered above.
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync(JobStatus.Completed, "phase2-renewed-race-requester");
        var expiredLock = await SeedExpiredLockRowAsync(lockName, jobQueueId);

        // Simulates state changing between the caller observing this lock as expired and
        // ReconcileAsync's own delete attempt — DeleteIfStillExpiredAsync must re-check live,
        // not trust the expiredLock snapshot it was handed.
        var renewedExpiry = DateTime.UtcNow.AddMinutes(5);
        await using (var context = CreateDbContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.job_lock SET expires_at = {renewedExpiry}
                WHERE job_name = {lockName} AND jobqueueid = {jobQueueId};");
        }

        try
        {
            var service = CreateService();

            await service.ReconcileAsync(expiredLock);

            await using var context = CreateDbContext();
            var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
            var remaining = await repository.GetLockAsync(lockName);

            Assert.NotNull(remaining);
            Assert.True(remaining!.ExpiresAt > DateTime.UtcNow, "Lock must be left untouched — it was no longer expired at delete time.");
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task ReconcileAsync_CalledTwiceForSameLock_SecondCallIsHarmless()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync(JobStatus.Running, "phase2-double-call-requester");
        var expiredLock = await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var service = CreateService();

            await service.ReconcileAsync(expiredLock);
            // Second call reuses the same (now stale) expiredLock snapshot — exactly what a
            // second concurrent reconciler racing on the same row would pass.
            var exception = await Record.ExceptionAsync(() => service.ReconcileAsync(expiredLock));

            Assert.Null(exception);

            await using var context = CreateDbContext();
            var persistedStatus = await context.Database
                .SqlQuery<string>($@"
                    SELECT s.status AS ""Value""
                    FROM fps.job_queue q
                    JOIN fps.job_status s ON s.statusid = q.statusid
                    WHERE q.jobqueueid = {jobQueueId}")
                .SingleAsync();

            Assert.Equal(nameof(JobStatus.Failed), persistedStatus);

            var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
            Assert.Null(await repository.GetLockAsync(lockName));
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    // ---------- Helpers ----------

    private sealed record PersistedRow(string Status, string? ErrorMessage, string RequestedBy);

    private static string UniqueLockName() => $"phase2-reconcile-test-{Guid.NewGuid():N}";

    private async Task<(Guid JobQueueId, Guid JobExecutionId)> SeedJobQueueRowAsync(JobStatus status, string requestedBy)
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.RecreateSummary}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = {status.ToString()}")
            .SingleAsync();

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
            VALUES
                ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, {requestedBy}, NOW(), NOW());");

        return (jobQueueId, jobExecutionId);
    }

    private async Task<BatchLock> SeedExpiredLockRowAsync(string lockName, Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        var acquiredAt = DateTime.UtcNow.AddMinutes(-15);
        var expiresAt = DateTime.UtcNow.AddMinutes(-10);

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, jobqueueid, is_active)
            VALUES ({acquiredAt}, {expiresAt}, {lockName}, {jobQueueId}, TRUE);");

        var repository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
        return (await repository.GetLockAsync(lockName))!;
    }

    private async Task CleanupAsync(string lockName, Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_lock WHERE job_name = {lockName};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue_log WHERE jobqueueid = {jobQueueId};");
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
    }

    private BatchLockReconciliationService CreateService(ILogger<BatchLockReconciliationService>? logger = null)
    {
        var context = CreateDbContext();
        var lockRepository = new BatchLockRepository(context, NullLogger<BatchLockRepository>.Instance);
        var executionRepository = new JobExecutionRepository(context, NullLogger<JobExecutionRepository>.Instance);
        return new BatchLockReconciliationService(lockRepository, executionRepository, logger ?? NullLogger<BatchLockReconciliationService>.Instance);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    /// <summary>Minimal in-memory logger capturing level + rendered message, so tests can assert what was logged.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
