using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Reconciles a single expired <c>job_lock</c> row against the <c>job_queue</c> execution it
/// references, so a crashed container's orphaned lock and stuck execution both self-heal.
/// Deliberately does not decide *which* locks are expired or scan for them — it operates only on
/// the lock row its caller hands it. Finding expired locks (a container's generic startup sweep,
/// or a scoped check immediately before acquiring one specific lock) is the caller's
/// responsibility, wired in a later phase.
/// </summary>
public interface IBatchLockReconciliationService
{
    /// <summary>
    /// Given a lock row the caller believes is expired, marks its owning execution Failed if it
    /// is still non-terminal (preserving the row's original <c>RequestedBy</c>), then deletes the
    /// lock — but only while it remains expired at delete time, so a lock that was renewed or
    /// replaced in the interim is never force-deleted. Safe to call more than once for the same
    /// lock: a second call finds the execution already terminal and the lock already gone, and
    /// both steps become harmless no-ops.
    /// <para>
    /// Enforces the expiry invariant itself rather than only trusting the caller — a lock that
    /// turns out not to be expired is refused (no-op) rather than acted on, so a future caller
    /// (this service has more than one entry point from a later phase) can never accidentally
    /// mark a healthy owner's execution Failed.
    /// </para>
    /// </summary>
    /// <param name="expiredLock">A lock row the caller believes is expired; refused as a no-op if it turns out not to be.</param>
    Task ReconcileAsync(BatchLock expiredLock, CancellationToken cancellationToken = default);
}
