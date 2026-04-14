using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// In-memory (no-op) implementation of <see cref="IBatchLockRepository"/> for local development.
/// Always grants the lock and silently no-ops on release.
/// This means concurrent runs are NOT prevented locally — that protection is only active
/// when a real database is configured (Production / AWS).
/// </summary>
public sealed class InMemoryBatchLockRepository : IBatchLockRepository
{
    private readonly ILogger<InMemoryBatchLockRepository> _logger;

    /// <summary>Initializes <see cref="InMemoryBatchLockRepository"/>.</summary>
    public InMemoryBatchLockRepository(ILogger<InMemoryBatchLockRepository> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<bool> TryAcquireLockAsync(string jobName, string runId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[LocalMode] Lock acquire skipped (in-memory) — JobName={JobName} RunId={RunId}", jobName, runId);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task ReleaseLockAsync(string jobName, string runId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[LocalMode] Lock release skipped (in-memory) — JobName={JobName} RunId={RunId}", jobName, runId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<BatchLock?>(null);
    }
}
