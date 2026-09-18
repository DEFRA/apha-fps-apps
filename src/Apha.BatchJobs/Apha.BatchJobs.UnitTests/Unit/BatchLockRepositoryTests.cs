using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Operational.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed tests for the Phase 1 lock-lease primitives added to
/// <see cref="BatchLockRepository"/>: <see cref="BatchLockRepository.GetLockAsync"/>,
/// <see cref="BatchLockRepository.GetExpiredLocksAsync"/>,
/// <see cref="BatchLockRepository.DeleteIfStillExpiredAsync"/>, and the strengthened
/// (ownership + non-expired) <see cref="BatchLockRepository.TryRenewLockAsync"/>. See
/// batchjobs-job-lock-lease-heartbeat-worker-implementation-spec-2026-09-18.md.
/// </summary>
[Trait("Category", "Integration")]
public sealed class BatchLockRepositoryTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public BatchLockRepositoryTests()
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
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------- GetLockAsync ----------

    [SkippableFact]
    public async Task GetLockAsync_WhenNoLockExists_ReturnsNull()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var repository = CreateRepository();

        var result = await repository.GetLockAsync(lockName);

        Assert.Null(result);
    }

    [SkippableFact]
    public async Task GetLockAsync_WhenActiveUnexpiredLockExists_ReturnsIt()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();

        try
        {
            var repository = CreateRepository();
            Assert.True(await repository.TryAcquireLockAsync(lockName, jobQueueId, timeoutSeconds: 300));

            var result = await repository.GetLockAsync(lockName);

            Assert.NotNull(result);
            Assert.Equal(jobQueueId, result!.JobQueueId);
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task GetLockAsync_WhenLockExistsButExpired_StillReturnsIt()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();
        await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var repository = CreateRepository();

            var result = await repository.GetLockAsync(lockName);

            Assert.NotNull(result);
            Assert.Equal(jobQueueId, result!.JobQueueId);
            Assert.True(result.ExpiresAt < DateTime.UtcNow, "GetLockAsync must not filter by expiry — that's GetActiveLockAsync's job.");
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    // ---------- GetExpiredLocksAsync ----------

    [SkippableFact]
    public async Task GetExpiredLocksAsync_ReturnsExpiredLock_ButNotAHealthyUnexpiredOne()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var expiredLockName = UniqueLockName();
        var healthyLockName = UniqueLockName();
        var (expiredJobQueueId, _) = await SeedJobQueueRowAsync();
        var (healthyJobQueueId, _) = await SeedJobQueueRowAsync();

        await SeedExpiredLockRowAsync(expiredLockName, expiredJobQueueId);

        try
        {
            var repository = CreateRepository();
            Assert.True(await repository.TryAcquireLockAsync(healthyLockName, healthyJobQueueId, timeoutSeconds: 300));

            var expiredLocks = await repository.GetExpiredLocksAsync();

            Assert.Contains(expiredLocks, l => l.JobName == expiredLockName && l.JobQueueId == expiredJobQueueId);
            Assert.DoesNotContain(expiredLocks, l => l.JobName == healthyLockName);
        }
        finally
        {
            await CleanupAsync(expiredLockName, expiredJobQueueId);
            await CleanupAsync(healthyLockName, healthyJobQueueId);
        }
    }

    // ---------- DeleteIfStillExpiredAsync ----------

    [SkippableFact]
    public async Task DeleteIfStillExpiredAsync_WhenStillExpired_DeletesAndReturnsTrue()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();
        await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var repository = CreateRepository();

            var deleted = await repository.DeleteIfStillExpiredAsync(lockName, jobQueueId);

            Assert.True(deleted);
            Assert.Null(await repository.GetLockAsync(lockName));
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task DeleteIfStillExpiredAsync_WhenAlreadyDeletedByAnotherReconciler_ReturnsFalse()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();
        await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var repository = CreateRepository();

            var firstDelete = await repository.DeleteIfStillExpiredAsync(lockName, jobQueueId);
            var secondDelete = await repository.DeleteIfStillExpiredAsync(lockName, jobQueueId);

            Assert.True(firstDelete);
            Assert.False(secondDelete, "A second reconciler racing on the same already-reconciled row must be a harmless no-op.");
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task DeleteIfStillExpiredAsync_WhenLeaseWasRenewedSinceBeingObservedAsExpired_ReturnsFalseAndLeavesRowIntact()
    {
        // Simulates the exact race DeleteIfStillExpiredAsync exists to close: reconciliation
        // observed the lock as expired earlier, but by the time it deletes, the owner has renewed
        // it. The DELETE's own WHERE clause re-checks expiry rather than trusting the earlier read.
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();
        await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            await using var context = CreateDbContext();
            var renewedExpiry = DateTime.UtcNow.AddMinutes(5);
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fps.job_lock SET expires_at = {renewedExpiry}
                WHERE job_name = {lockName} AND jobqueueid = {jobQueueId};");

            var repository = CreateRepository();

            var deleted = await repository.DeleteIfStillExpiredAsync(lockName, jobQueueId);

            Assert.False(deleted);
            var remaining = await repository.GetLockAsync(lockName);
            Assert.NotNull(remaining);
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    // ---------- TryRenewLockAsync — the three required ownership/expiry cases ----------

    [SkippableFact]
    public async Task TryRenewLockAsync_CorrectOwnerAndValidLease_RenewsAndReturnsTrue()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();

        try
        {
            var repository = CreateRepository();
            // Acquire with a short lease, then renew with a much longer one — makes the
            // before/after expires_at comparison unambiguous regardless of the column's
            // timestamp precision (Postgres truncates to whole seconds), rather than relying on
            // wall-clock drift between two calls that execute almost back-to-back.
            Assert.True(await repository.TryAcquireLockAsync(lockName, jobQueueId, timeoutSeconds: 30));
            var beforeRenewal = await repository.GetLockAsync(lockName);

            var renewed = await repository.TryRenewLockAsync(lockName, jobQueueId, timeoutSeconds: 300);

            Assert.True(renewed);
            var afterRenewal = await repository.GetLockAsync(lockName);
            Assert.True(
                afterRenewal!.ExpiresAt > beforeRenewal!.ExpiresAt,
                "Renewal must push expires_at further into the future.");
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task TryRenewLockAsync_CorrectOwnerButExpiredLease_ReturnsFalse()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (jobQueueId, _) = await SeedJobQueueRowAsync();
        await SeedExpiredLockRowAsync(lockName, jobQueueId);

        try
        {
            var repository = CreateRepository();

            var renewed = await repository.TryRenewLockAsync(lockName, jobQueueId, timeoutSeconds: 300);

            Assert.False(
                renewed,
                "An expired lease is a dead fixed point — even its own correct owner must not be able to revive it.");

            var stillExpired = await repository.GetLockAsync(lockName);
            Assert.NotNull(stillExpired);
            Assert.True(stillExpired!.ExpiresAt < DateTime.UtcNow, "Row must be left untouched by the failed renewal attempt.");
        }
        finally
        {
            await CleanupAsync(lockName, jobQueueId);
        }
    }

    [SkippableFact]
    public async Task TryRenewLockAsync_WrongOwnerValidLease_ReturnsFalse()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var lockName = UniqueLockName();
        var (correctOwnerJobQueueId, _) = await SeedJobQueueRowAsync();
        var wrongOwnerJobQueueId = Guid.NewGuid();

        try
        {
            var repository = CreateRepository();
            Assert.True(await repository.TryAcquireLockAsync(lockName, correctOwnerJobQueueId, timeoutSeconds: 300));
            var beforeAttempt = await repository.GetLockAsync(lockName);

            var renewed = await repository.TryRenewLockAsync(lockName, wrongOwnerJobQueueId, timeoutSeconds: 300);

            Assert.False(renewed, "A non-owner must never be able to renew someone else's held lock.");

            var afterAttempt = await repository.GetLockAsync(lockName);
            Assert.Equal(beforeAttempt!.ExpiresAt, afterAttempt!.ExpiresAt);
            Assert.Equal(correctOwnerJobQueueId, afterAttempt.JobQueueId);
        }
        finally
        {
            await CleanupAsync(lockName, correctOwnerJobQueueId);
        }
    }

    // ---------- Helpers ----------

    private static string UniqueLockName() => $"phase1-lock-test-{Guid.NewGuid():N}";

    private async Task<(Guid JobQueueId, Guid JobExecutionId)> SeedJobQueueRowAsync()
    {
        await using var context = CreateDbContext();

        var jobId = await context.Database
            .SqlQuery<int>($@"SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.RecreateSummary}")
            .SingleAsync();

        var statusId = await context.Database
            .SqlQuery<int>($@"SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Initiated'")
            .SingleAsync();

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
            VALUES
                ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, 'phase1-lock-repo-test', NOW(), NOW());");

        return (jobQueueId, jobExecutionId);
    }

    private async Task SeedExpiredLockRowAsync(string lockName, Guid jobQueueId)
    {
        await using var context = CreateDbContext();
        var expiredAt = DateTime.UtcNow.AddMinutes(-10);
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, jobqueueid, is_active)
            VALUES ({expiredAt.AddMinutes(-5)}, {expiredAt}, {lockName}, {jobQueueId}, TRUE);");
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

    private BatchLockRepository CreateRepository() => new(CreateDbContext(), NullLogger<BatchLockRepository>.Instance);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
