using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

        // Attempt atomic insert without raising an error on contention.
        // With uq_job_lock_job_name_active (partial unique on active rows),
        // this returns 1 when lock is acquired and 0 when already held.
        var insertedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, run_id, is_active)
            VALUES ({now}, {now.AddSeconds(timeoutSeconds)}, {jobName}, {runId}, TRUE)
            ON CONFLICT DO NOTHING;", cancellationToken);

        if (insertedRows > 0)
        {
            _logger.LogInformation("Lock acquired | JobName={JobName} | RunId={RunId}", jobName, runId);
            return true;
        }

        _context.ChangeTracker.Clear();
        _logger.LogInformation("Lock contention detected | JobName={JobName} | RunId={RunId}", jobName, runId);
        return false;
    }

    /// <inheritdoc />
    public async Task ReleaseLockAsync(string jobName, string runId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (string.IsNullOrWhiteSpace(runId))
            throw new ArgumentException("Run ID cannot be null or empty.", nameof(runId));

        // Guard against stale tracking state leaking from prior operations in the
        // same scoped DbContext (e.g., failed job step writes).
        _context.ChangeTracker.Clear();

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
