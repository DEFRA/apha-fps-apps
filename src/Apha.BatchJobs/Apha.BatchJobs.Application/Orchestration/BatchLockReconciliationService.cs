using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.Application.Orchestration;

/// <inheritdoc cref="IBatchLockReconciliationService"/>
public sealed class BatchLockReconciliationService : IBatchLockReconciliationService
{
    /// <summary>
    /// Business-facing message for an execution reconciled after its lock lease expired.
    /// Deliberately generic — lease expiry proves only that the owner stopped renewing, not the
    /// cause (could be a container crash, but equally a prolonged DB/connectivity failure or a
    /// stalled process).
    /// </summary>
    internal const string ReconciledErrorMessage = "Execution marked Failed because its worker lock lease expired.";

    /// <summary>Diagnostic-only counterpart to <see cref="ReconciledErrorMessage"/> — never shown to business users.</summary>
    internal const string ReconciledDiagnosticSummary = "Orphaned execution reconciled after job_lock lease expiry.";

    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<BatchLockReconciliationService> _logger;

    public BatchLockReconciliationService(
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        ILogger<BatchLockReconciliationService>? logger = null)
    {
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? NullLogger<BatchLockReconciliationService>.Instance;
    }

    /// <inheritdoc />
    public async Task ReconcileAsync(BatchLock expiredLock, CancellationToken cancellationToken = default)
    {
        if (expiredLock is null)
            throw new ArgumentNullException(nameof(expiredLock));

        // Defensive, not merely trusting the caller: Phase 3 will call this from two different
        // entry points (a generic startup sweep and a scoped pre-acquire check). Enforcing the
        // expiry invariant here, not just in each caller, means a future caller cannot
        // accidentally mark a healthy owner's execution Failed by passing a lock that turns out
        // not to be expired.
        if (expiredLock.ExpiresAt >= DateTime.UtcNow)
        {
            _logger.LogDebug(
                "ReconcileAsync called with a lock that is not expired — refusing, no-op | LockName={LockName} | JobQueueId={JobQueueId} | ExpiresAt={ExpiresAt}",
                expiredLock.JobName,
                expiredLock.JobQueueId,
                expiredLock.ExpiresAt);
            return;
        }

        var execution = await _executionRepository.GetExecutionByJobQueueIdAsync(expiredLock.JobQueueId, cancellationToken);

        if (execution is null)
        {
            _logger.LogWarning(
                "Orphan reconciliation found an expired lock with no matching job_queue row | LockName={LockName} | JobQueueId={JobQueueId} | ExpiresAt={ExpiresAt}",
                expiredLock.JobName,
                expiredLock.JobQueueId,
                expiredLock.ExpiresAt);
        }
        else if (IsNonTerminal(execution.Status))
        {
            var previousStatus = execution.Status;

            execution.Status = JobStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.ErrorMessage = ReconciledErrorMessage;
            execution.DiagnosticSummary = ReconciledDiagnosticSummary;
            // execution.UserId is already populated from GetExecutionByJobQueueIdAsync's own read
            // of this row's RequestedBy — passed straight through so UpdateExecutionRecordAsync
            // does not overwrite the original requester's identity with anything synthetic.

            await _executionRepository.UpdateExecutionRecordAsync(execution, cancellationToken);

            _logger.LogWarning(
                "Reconciled orphaned execution | LockName={LockName} | JobQueueId={JobQueueId} | JobExecutionId={JobExecutionId} | PreviousStatus={PreviousStatus} | RequestedBy={RequestedBy}",
                expiredLock.JobName,
                expiredLock.JobQueueId,
                execution.JobExecutionId,
                previousStatus,
                execution.UserId);
        }
        else
        {
            _logger.LogInformation(
                "Expired lock's execution is already terminal — status left unchanged | LockName={LockName} | JobQueueId={JobQueueId} | Status={Status}",
                expiredLock.JobName,
                expiredLock.JobQueueId,
                execution.Status);
        }

        // Conditional on the lock still being expired at delete time — never a forced delete.
        // If it was renewed or replaced since expiredLock was observed, this is a no-op.
        var deleted = await _lockRepository.DeleteIfStillExpiredAsync(expiredLock.JobName, expiredLock.JobQueueId, cancellationToken);

        if (deleted)
        {
            _logger.LogInformation(
                "Expired lock deleted | LockName={LockName} | JobQueueId={JobQueueId}",
                expiredLock.JobName,
                expiredLock.JobQueueId);
        }
        else
        {
            _logger.LogInformation(
                "Expired lock was no longer expired at delete time (renewed or already removed) — left untouched | LockName={LockName} | JobQueueId={JobQueueId}",
                expiredLock.JobName,
                expiredLock.JobQueueId);
        }
    }

    // Initiated/Approved/Running: a lock can exist for a row not yet transitioned to Running (the
    // lock is acquired before JobOrchestrator's Approved/Initiated -> Running write), so treating
    // only "Running" as reconcilable would miss a crash in that narrow window.
    private static bool IsNonTerminal(JobStatus status) =>
        status is JobStatus.Initiated or JobStatus.Approved or JobStatus.Running;
}
