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
        await _execRepo.DidNotReceive().UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
        await _lockRepo.DidNotReceive().DeleteIfStillExpiredAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_NoMatchingExecution_DoesNotCallUpdate_ButStillAttemptsConditionalDelete()
    {
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>())
            .Returns((JobExecutionRecord?)null);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(true);

        await _service.ReconcileAsync(expiredLock);

        await _execRepo.DidNotReceive().UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
        await _lockRepo.Received(1).DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(JobStatus.Initiated)]
    [InlineData(JobStatus.Approved)]
    [InlineData(JobStatus.Running)]
    public async Task ReconcileAsync_NonTerminalExecution_MarksFailedWithConfirmedMessagesAndPreservesRequestedBy(JobStatus nonTerminalStatus)
    {
        // Initiated/Approved are included, not just Running — a lock can exist for a row not yet
        // transitioned to Running (JobOrchestrator acquires the lock before writing that
        // transition), so a crash in that narrow window must be reconcilable too.
        const string originalRequestedBy = "original-requester-must-survive";
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        var execution = new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = "SomeLock",
            JobExecutionId = Guid.NewGuid(),
            JobQueueId = jobQueueId,
            UserId = originalRequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = nonTerminalStatus,
            StartedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>()).Returns(execution);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>()).Returns(true);

        JobExecutionRecord? updatedWith = null;
        _execRepo.UpdateExecutionRecordAsync(
                Arg.Do<JobExecutionRecord>(r => updatedWith = r),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _service.ReconcileAsync(expiredLock);

        await _execRepo.Received(1).UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
        Assert.NotNull(updatedWith);
        Assert.Equal(JobStatus.Failed, updatedWith!.Status);
        Assert.Equal(BatchLockReconciliationService.ReconciledErrorMessage, updatedWith.ErrorMessage);
        Assert.Equal(BatchLockReconciliationService.ReconciledDiagnosticSummary, updatedWith.DiagnosticSummary);
        Assert.Equal(originalRequestedBy, updatedWith.UserId);
        await _lockRepo.Received(1).DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    [InlineData(JobStatus.Rejected)]
    public async Task ReconcileAsync_TerminalExecution_DoesNotCallUpdate_ButStillAttemptsConditionalDelete(JobStatus terminalStatus)
    {
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
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>()).Returns(execution);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>()).Returns(true);

        await _service.ReconcileAsync(expiredLock);

        await _execRepo.DidNotReceive().UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
        await _lockRepo.Received(1).DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_DeleteReturnsFalse_CompletesWithoutThrowing_NeverForceDeletes()
    {
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>())
            .Returns((JobExecutionRecord?)null);
        // Simulates the lock having been renewed/replaced before ReconcileAsync's own delete ran.
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>())
            .Returns(false);

        var exception = await Record.ExceptionAsync(() => _service.ReconcileAsync(expiredLock));

        Assert.Null(exception);
        // The only delete path used is the conditional one — never ReleaseLockAsync or any other
        // unconditional removal.
        await _lockRepo.DidNotReceive().ReleaseLockAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReconcileAsync_CalledTwiceForSameLock_SecondCallFindsAlreadyReconciledStateAndIsHarmless()
    {
        // Models what a second reconciler (or a re-run against the same stale expiredLock
        // snapshot) actually observes after the first call already reconciled this row: the
        // execution is now Failed (terminal) and the lock is already gone.
        var expiredLock = BuildLock("SomeLock", out var jobQueueId);
        var alreadyReconciledExecution = new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = "SomeLock",
            JobExecutionId = Guid.NewGuid(),
            JobQueueId = jobQueueId,
            UserId = "original-requester",
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = JobStatus.Failed,
            StartedAt = DateTime.UtcNow.AddMinutes(-20),
            ErrorMessage = BatchLockReconciliationService.ReconciledErrorMessage
        };
        _execRepo.GetExecutionByJobQueueIdAsync(jobQueueId, Arg.Any<CancellationToken>()).Returns(alreadyReconciledExecution);
        _lockRepo.DeleteIfStillExpiredAsync("SomeLock", jobQueueId, Arg.Any<CancellationToken>()).Returns(false);

        var exception = await Record.ExceptionAsync(() => _service.ReconcileAsync(expiredLock));

        Assert.Null(exception);
        await _execRepo.DidNotReceive().UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
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
