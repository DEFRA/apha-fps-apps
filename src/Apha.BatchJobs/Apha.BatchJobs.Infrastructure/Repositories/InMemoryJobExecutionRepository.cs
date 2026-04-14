using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of <see cref="IJobExecutionRepository"/> for local development.
/// Stores records in a simple list so execution history is visible in logs during local runs.
/// Records are lost when the process exits — this is intentional for local use only.
/// </summary>
public sealed class InMemoryJobExecutionRepository : IJobExecutionRepository
{
    private readonly List<JobExecutionRecord> _records = [];
    private int _nextId = 1;
    private readonly ILogger<InMemoryJobExecutionRepository> _logger;

    /// <summary>Initializes <see cref="InMemoryJobExecutionRepository"/>.</summary>
    public InMemoryJobExecutionRepository(ILogger<InMemoryJobExecutionRepository> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        record.ExecutionId = _nextId++;
        _records.Add(record);
        _logger.LogDebug("[LocalMode] Execution record created (in-memory) — ExecutionId={ExecutionId} JobName={JobName} Status={Status}",
            record.ExecutionId, record.JobName, record.Status);
        return Task.FromResult(record.ExecutionId);
    }

    /// <inheritdoc />
    public Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[LocalMode] Execution record updated (in-memory) — ExecutionId={ExecutionId} Status={Status} Duration={Duration}s",
            record.ExecutionId, record.Status, record.DurationSeconds);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<JobExecutionRecord?> GetExecutionRecordAsync(int executionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_records.FirstOrDefault(r => r.ExecutionId == executionId));
    }

    /// <inheritdoc />
    public Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            _records
                .Where(r => r.JobName == jobName)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefault());
    }
}
