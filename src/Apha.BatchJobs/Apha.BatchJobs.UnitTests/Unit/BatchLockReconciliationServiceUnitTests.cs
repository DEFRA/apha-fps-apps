using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Fast, DB-independent tests for <see cref="BatchLockReconciliationService"/>'s own branching
/// logic, mocking <see cref="IBatchLockRepository"/>/<see cref="IJobExecutionRepository"/> the
/// same way <c>JobOrchestratorTests</c> does. Runs unconditionally in CI (not tagged
/// Integration), unlike the companion Postgres-backed
/// <c>BatchLockReconciliationServiceTests</c>, which proves the same behaviour end-to-end against
/// a real database when one is available.
/// </summary>
public sealed class BatchLockReconciliationServiceUnitTests
{
    private readonly IBatchLockRepository _lockRepo = Substitute.For<IBatchLockRepository>();
    private readonly IJobExecutionRepository _execRepo = Substitute.For<IJobExecutionRepository>();
    private readonly BatchLockReconciliationService _service;

    public BatchLockReconciliationServiceUnitTests()
    {
        _service = new BatchLockReconciliationService(_lockRepo, _execRepo, NullLogger<BatchLockReconciliationService>.Instance);
    }

    [Fact]
    public async Task ReconcileAsync_NullExpiredLock_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ReconcileAsync(null!));
    }

    [Fact]
    public async Task ReconcileAsync_LockNotActuallyExpired_RefusesAsNoOp_NoUpdateNoDeleteAttempt()
    {
        // Defensive invariant: ReconcileAsync must not trust the caller blindly — a lock that
        // turns out not to be expired must never be acted on, since Phase 3 will call this from
        // two different entry points and a bug in either must not be able to mark a healthy
        // owner's execution Failed.
        var notExpiredLock = BuildLock("SomeLock", out var jobQueueId, expiresAt: DateTime.UtcNow.AddMinutes(10));

        await _service.ReconcileAsync(notExpiredLock);

        await _execRepo.DidNotReceive().GetExecutionByJobQueueIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _execRepo.DidNotReceive().MarkFailedIfNonTerminalAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _lockRepo.DidNotReceive().DeleteIfStillExpiredAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_ClaimFails_NeverTouchesExecutionRepository()
    {
        // The critical regression case for the lease-renewal race: if the lock claim fails
        // (renewed or already reconciled by someone else), the execution row must never be read
        // or written — the old buggy order touched execution.Status before this check.
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await Record.ExceptionAsync(() => _service.ReconcileAsync(expiredLock));

        Assert.Null(exception);
        await _execRepo.DidNotReceive().GetExecutionByJobQueueIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _execRepo.DidNotReceive().MarkFailedIfNonTerminalAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        // The only delete path used is the conditional one — never ReleaseLockAsync or any other
        // unconditional removal.
        await _lockRepo.DidNotReceive().ReleaseLockAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_ClaimSucceedsButNoMatchingExecution_ReadsForExplanationOnly()
    {
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(true);
        // MarkFailedIfNonTerminalAsync itself returns false when no row exists to update.
        _execRepo.MarkFailedIfNonTerminalAsync(jobQueueId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>())
            .Returns((JobExecutionRecord?)null);

        await _service.ReconcileAsync(expiredLock);

        await _lockRepo.Received(1).DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>());
        await _execRepo.Received(1).MarkFailedIfNonTerminalAsync(jobQueueId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        // The read only happens because the atomic update found nothing — it's for the log
        // message, not for deciding whether to write.
        await _execRepo.Received(1).GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_AtomicMarkSucceeds_LogsReconciledAndNeverReadsForExplanation()
    {
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.MarkFailedIfNonTerminalAsync(
                jobQueueId,
                BatchLockReconciliationService.ReconciledErrorMessage,
                BatchLockReconciliationService.ReconciledDiagnosticSummary,
                Arg.Any<CancellationToken>())
            .Returns(true);

        await _service.ReconcileAsync(expiredLock);

        await _execRepo.Received(1).MarkFailedIfNonTerminalAsync(
            jobQueueId,
            BatchLockReconciliationService.ReconciledErrorMessage,
            BatchLockReconciliationService.ReconciledDiagnosticSummary,
            Arg.Any<CancellationToken>());
        // No stale read participates in — or follows — a successful atomic reconciliation.
        await _execRepo.DidNotReceive().GetExecutionByJobQueueIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    [InlineData(JobStatus.Rejected)]
    public async Task ReconcileAsync_AtomicMarkReturnsFalse_AndExecutionIsTerminal_CompletesWithoutThrowing(JobStatus terminalStatus)
    {
        // From the service's perspective this covers both a row that was already terminal when
        // observed and one that became terminal independently between the lock claim and the
        // atomic write — MarkFailedIfNonTerminalAsync returning false is the only signal either
        // way, by design: no stale status participates in the decision.
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        var execution = new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = "SomeLock",
            JobExecutionId = Guid.NewGuid(),
            JobQueueId = jobQueueId,
            UserId = "irrelevant-here",
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = terminalStatus,
            StartedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>()).Returns(true);
        _execRepo.MarkFailedIfNonTerminalAsync(jobQueueId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>()).Returns(execution);

        var exception = await Record.ExceptionAsync(() => _service.ReconcileAsync(expiredLock));

        Assert.Null(exception);
        await _execRepo.Received(1).GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_CalledTwiceForSameLock_SecondCallIsHarmless()
    {
        // First call claims the lock and reconciles; second call (a re-run against the same
        // stale expiredLock snapshot, or a second concurrent reconciler) finds the lock already
        // gone and must be a pure no-op — it must not re-touch the execution row at all.
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(true, false);
        _execRepo.MarkFailedIfNonTerminalAsync(jobQueueId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        await _service.ReconcileAsync(expiredLock);
        var exception = await Record.ExceptionAsync(() => _service.ReconcileAsync(expiredLock));

        Assert.Null(exception);
        await _execRepo.Received(1).MarkFailedIfNonTerminalAsync(jobQueueId, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static BatchLock BuildLock(string lockName, out Guid jobQueueId, DateTime? expiresAt = null)
    {
        jobQueueId = Guid.NewGuid();
        return new BatchLock
        {
            LockId = 1,
            JobName = lockName,
            AcquiredAt = DateTime.UtcNow.AddMinutes(-15),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(-10),
            JobQueueId = jobQueueId,
            IsActive = true
        };
    }
}
