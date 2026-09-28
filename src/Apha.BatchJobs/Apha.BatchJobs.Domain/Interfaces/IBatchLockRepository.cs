using Apha.BatchJobs.Domain.Entities;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>Repository for managing batch job execution locks.</summary>
public interface IBatchLockRepository
{
    /// <summary>Attempts to acquire a distributed lock for a lock name (the resolved concurrency key — see <c>JobOrchestrator.ResolveLockName</c>; several job names may share one lock name, e.g. Year End Data Setup/CutOver).</summary>
    /// <param name="lockName">The resolved lock name requiring a lock.</param>
    /// <param name="jobQueueId">Unique UUID for the job queue execution entry.</param>
    /// <param name="timeoutSeconds">Lock timeout duration.</param>
    /// <returns>True if lock acquired; false if already locked by another process.</returns>
    Task<bool> TryAcquireLockAsync(string lockName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default);

    /// <summary>Releases a lock held by the given job queue ID.</summary>
    /// <param name="lockName">The resolved lock name.</param>
    /// <param name="jobQueueId">The job queue UUID holding the lock.</param>
    Task ReleaseLockAsync(string lockName, Guid jobQueueId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews a held lock for finite-timeout policies. Ownership-specific and non-renewable once
    /// expired: requires both the correct owning <paramref name="jobQueueId"/> AND that the lease
    /// has not already lapsed. A lease that has expired is a dead fixed point — the only thing
    /// that could ever push <c>expires_at</c> back into the future is a successful renewal, which
    /// this same condition blocks once expired, so a resurrected owner can never revive a lease
    /// that orphan reconciliation has already started treating as dead.
    /// </summary>
    /// <param name="lockName">The resolved lock name.</param>
    /// <param name="jobQueueId">The job queue UUID holding the lock.</param>
    /// <param name="timeoutSeconds">New timeout window in seconds.</param>
    /// <returns>True when a lock row was renewed or lock timeout is unlimited; false if the lock row is missing, owned by a different execution, or its lease has already expired.</returns>
    Task<bool> TryRenewLockAsync(string lockName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default);

    /// <summary>Gets the current active-and-unexpired lock for a lock name, if one exists. Unchanged — see <see cref="GetLockAsync"/> for a lookup that also returns an expired row.</summary>
    /// <param name="lockName">The resolved lock name.</param>
    Task<BatchLock?> GetActiveLockAsync(string lockName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current lock row for a lock name regardless of whether its lease has expired, or
    /// null if no lock is held for this name at all. Deliberately separate from
    /// <see cref="GetActiveLockAsync"/> rather than relaxing that method's existing
    /// active-and-unexpired semantics — reconciliation's need to see an expired row is a distinct
    /// concern from "is there a usable lock right now."
    /// </summary>
    /// <param name="lockName">The resolved lock name.</param>
    Task<BatchLock?> GetLockAsync(string lockName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every currently-expired active lock row, system-wide, for the worker's generic
    /// startup reconciliation sweep. Bounded and cheap — <c>job_lock</c> holds at most one active
    /// row per lock name.
    /// </summary>
    Task<IReadOnlyList<BatchLock>> GetExpiredLocksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes one specific lock row, but only if it is still active and expired at delete time
    /// (re-checked in the same statement, not against a stale earlier read) — keeping this
    /// consistent with every other lease predicate in this interface: <see cref="GetLockAsync"/>
    /// is active; <see cref="GetExpiredLocksAsync"/> is active + expired; <see
    /// cref="TryRenewLockAsync"/> is active + owner + unexpired; this is active + owner + expired.
    /// Idempotent: a second concurrent caller — another reconciler that already won the race, or
    /// the lock's own owner having since renewed it — gets <c>false</c> and does nothing further.
    /// </summary>
    /// <param name="lockName">The resolved lock name.</param>
    /// <param name="jobQueueId">The job queue UUID the expired lock row was found to reference.</param>
    /// <returns>True if a still-expired row was found and deleted; false otherwise.</returns>
    Task<bool> DeleteIfStillExpiredAsync(string lockName, Guid jobQueueId, CancellationToken cancellationToken = default);
}
