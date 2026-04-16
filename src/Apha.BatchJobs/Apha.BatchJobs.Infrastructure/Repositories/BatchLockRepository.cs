using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// Implementation of batch lock repository using EF Core.
/// </summary>
public class BatchLockRepository : IBatchLockRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<BatchLockRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the BatchLockRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for structured lock lifecycle events.</param>
    public BatchLockRepository(BatchJobsDbContext context, ILogger<BatchLockRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? NullLogger<BatchLockRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireLockAsync(string jobName, string runId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or empty.", nameof(runId));

        var now = DateTime.UtcNow;
        _logger.LogInformation(
            "Lock acquisition requested | JobName={JobName} | RunId={RunId} | TimeoutSeconds={TimeoutSeconds}",
            jobName,
            runId,
            timeoutSeconds);

        // Remove expired locks first so the unique partial index slot is freed.
        await _context.BatchLocks
            .Where(l => l.JobName == jobName && l.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        // Attempt atomic INSERT. The DB-level partial unique index on
        // (job_name) WHERE is_active = TRUE means only one row per job can
        // exist with is_active = TRUE. A concurrent inserter receives a
        // unique constraint violation (Postgres SqlState 23505) rather than
        // silently winning the race.
        var newLock = new BatchLock
        {
            LockId = 0,
            JobName = jobName,
            RunId = runId,
            AcquiredAt = now,
            ExpiresAt = now.AddSeconds(timeoutSeconds),
            IsActive = true
        };

        _context.BatchLocks.Add(newLock);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Lock acquired | JobName={JobName} | RunId={RunId}", jobName, runId);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Another process holds an active lock for this job.
            _context.ChangeTracker.Clear();
            _logger.LogInformation("Lock contention detected | JobName={JobName} | RunId={RunId}", jobName, runId);
            return false;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == "23505";

    /// <inheritdoc />
    public async Task ReleaseLockAsync(string jobName, string runId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or empty.", nameof(runId));

        var lockToRelease = await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName && l.RunId == runId, cancellationToken);

        if (lockToRelease != null)
        {
            _context.BatchLocks.Remove(lockToRelease);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Lock released | JobName={JobName} | RunId={RunId}", jobName, runId);
        }
        else
        {
            _logger.LogInformation("No lock found to release | JobName={JobName} | RunId={RunId}", jobName, runId);
        }
    }

    /// <inheritdoc />
    public async Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var now = DateTime.UtcNow;
        return await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName && l.IsActive && l.ExpiresAt > now, cancellationToken);
    }
}
