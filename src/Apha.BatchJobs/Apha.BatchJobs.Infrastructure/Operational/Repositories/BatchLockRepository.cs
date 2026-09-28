using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.Infrastructure.Operational.Repositories;

/// <summary>
/// Implementation of batch lock repository using EF Core.
/// </summary>
public class BatchLockRepository : IBatchLockRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<BatchLockRepository> _logger;

    public BatchLockRepository(BatchJobsDbContext context, ILogger<BatchLockRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? NullLogger<BatchLockRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireLockAsync(string lockName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        var now = DateTime.UtcNow;
        _logger.LogInformation(
            "Lock acquisition requested | LockName={LockName} | JobQueueId={JobQueueId} | TimeoutSeconds={TimeoutSeconds}",
            lockName,
            jobQueueId,
            timeoutSeconds);

        // Remove expired locks first so the unique partial index slot is freed.
        await _context.BatchLocks
            .Where(l => l.JobName == lockName && l.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        // Atomic insert; ON CONFLICT relies on uq_job_lock_job_name_active (partial unique on
        // active rows) — returns 1 row inserted when acquired, 0 when already held.
        var expiresAt = timeoutSeconds > 0
            ? now.AddSeconds(timeoutSeconds)
            : DateTime.MaxValue;

        var insertedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, jobqueueid, is_active)
            VALUES ({now}, {expiresAt}, {lockName}, {jobQueueId}, TRUE)
            ON CONFLICT DO NOTHING;", cancellationToken);

        if (insertedRows > 0)
        {
            _logger.LogInformation("Lock acquired | LockName={LockName} | JobQueueId={JobQueueId}", lockName, jobQueueId);
            return true;
        }

        _context.ChangeTracker.Clear();
        _logger.LogInformation("Lock contention detected | LockName={LockName} | JobQueueId={JobQueueId}", lockName, jobQueueId);
        return false;
    }

    /// <inheritdoc />
    public async Task ReleaseLockAsync(string lockName, Guid jobQueueId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        // Guards against stale tracking state from prior operations in this scoped DbContext.
        _context.ChangeTracker.Clear();

        var lockToRelease = await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == lockName && l.JobQueueId == jobQueueId, cancellationToken);

        if (lockToRelease != null)
        {
            _context.BatchLocks.Remove(lockToRelease);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Lock released | LockName={LockName} | JobQueueId={JobQueueId}", lockName, jobQueueId);
        }
        else
        {
            _logger.LogInformation("No lock found to release | LockName={LockName} | JobQueueId={JobQueueId}", lockName, jobQueueId);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TryRenewLockAsync(string lockName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        // Unlimited timeout uses a non-expiring lease marker and does not require periodic renewal.
        if (timeoutSeconds <= 0)
        {
            return true;
        }

        var now = DateTime.UtcNow;
        var nextExpiresAt = now.AddSeconds(timeoutSeconds);

        // Ownership-specific AND non-renewable once expired: an expired lease is a dead fixed
        // point — the only thing that could ever push ExpiresAt back into the future is a
        // successful renewal, which this same "ExpiresAt > now" condition blocks once expired.
        // A resurrected/delayed owner can therefore never revive a lease that has already lapsed,
        // regardless of how its renewal call interleaves against a concurrent reconciler.
        var updatedRows = await _context.BatchLocks
            .Where(l => l.JobName == lockName && l.JobQueueId == jobQueueId && l.IsActive && l.ExpiresAt > now)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(l => l.ExpiresAt, _ => nextExpiresAt),
                cancellationToken);

        return updatedRows > 0;
    }

    /// <inheritdoc />
    public async Task<BatchLock?> GetActiveLockAsync(string lockName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        var now = DateTime.UtcNow;
        var activeLock = await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == lockName && l.IsActive && l.ExpiresAt > now, cancellationToken);

        if (activeLock is null)
        {
            return null;
        }

        var queueState = await (
            from q in _context.TblJobQueue
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobQueueId == activeLock.JobQueueId
            select s.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.Equals(queueState, "Running", StringComparison.OrdinalIgnoreCase)
            || string.Equals(queueState, "Pending", StringComparison.OrdinalIgnoreCase)
            || string.Equals(queueState, "Retry", StringComparison.OrdinalIgnoreCase))
        {
            return activeLock;
        }

        _logger.LogWarning(
            "Detected stale lock; releasing lock row | LockName={LockName} | JobQueueId={JobQueueId} | QueueState={QueueState}",
            lockName,
            activeLock.JobQueueId,
            queueState ?? "MissingQueueRecord");

        _context.BatchLocks.Remove(activeLock);
        await _context.SaveChangesAsync(cancellationToken);
        return null;
    }

    /// <inheritdoc />
    public async Task<BatchLock?> GetLockAsync(string lockName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        // No ExpiresAt filter — deliberately returns the row whether expired or not, unlike
        // GetActiveLockAsync. Used by Layer 2 reconciliation, which needs to see an expired row
        // rather than have it look identical to "no lock held at all".
        //
        // AsNoTracking is deliberate: this is a pure read with no follow-up mutation through the
        // returned entity (unlike GetActiveLockAsync's self-heal path, which calls
        // BatchLocks.Remove on what it returns and therefore needs tracking). Without it, a
        // second call against the same DbContext after an ExecuteUpdateAsync/ExecuteDeleteAsync
        // elsewhere would return the stale pre-update instance from EF's identity map instead of
        // re-querying — those bulk operations bypass the change tracker entirely, so it never
        // learns the row changed underneath it.
        return await _context.BatchLocks
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.JobName == lockName && l.IsActive, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BatchLock>> GetExpiredLocksAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // job_lock holds at most one active row per lock name (partial unique index), so this is
        // a bounded, cheap read regardless of how many jobs exist — safe to run at every
        // container startup (Layer 1 generic reconciliation sweep). AsNoTracking — same
        // reasoning as GetLockAsync: pure read, no follow-up mutation through the returned
        // entities themselves.
        return await _context.BatchLocks
            .AsNoTracking()
            .Where(l => l.IsActive && l.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteIfStillExpiredAsync(string lockName, Guid jobQueueId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lockName))
            throw new ArgumentException("Lock name cannot be null or empty.", nameof(lockName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        var now = DateTime.UtcNow;

        // Re-checks expiry (and IsActive, keeping this consistent with every other lease
        // predicate below) in this same statement rather than trusting an earlier read — makes
        // concurrent reconciliation attempts idempotent: a losing caller (another reconciler that
        // already deleted this row, or the lock's own owner having since renewed it) simply
        // deletes 0 rows and gets false back.
        var deletedRows = await _context.BatchLocks
            .Where(l => l.JobName == lockName && l.JobQueueId == jobQueueId && l.IsActive && l.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedRows > 0;
    }
}
