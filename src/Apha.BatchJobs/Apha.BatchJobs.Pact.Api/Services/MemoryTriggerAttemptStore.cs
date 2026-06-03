using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class MemoryTriggerAttemptStore : ITriggerAttemptStore
{
    private const string AttemptPrefix = "pact-trigger-attempt:";
    private const string LatestByJobPrefix = "pact-trigger-latest-job:";

    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _entryTtl;

    public MemoryTriggerAttemptStore(
        IMemoryCache memoryCache,
        IOptions<TriggerStoreOptions> options)
    {
        _memoryCache = memoryCache;
        var configuredTtlMinutes = options.Value.EntryTtlMinutes;
        _entryTtl = TimeSpan.FromMinutes(configuredTtlMinutes > 0 ? configuredTtlMinutes : 60);
    }

    public Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default)
    {
        var attemptKey = BuildAttemptKey(record.JobExecutionId);
        var latestByJobKey = BuildLatestByJobKey(record.JobName);
        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _entryTtl
        };

        _memoryCache.Set(attemptKey, record, entryOptions);
        _memoryCache.Set(latestByJobKey, record.JobExecutionId, entryOptions);

        return Task.CompletedTask;
    }

    public Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobExecutionId))
        {
            return Task.FromResult<TriggerAttemptRecord?>(null);
        }

        var attemptKey = BuildAttemptKey(jobExecutionId);
        var result = _memoryCache.TryGetValue(attemptKey, out TriggerAttemptRecord? record)
            ? record
            : null;

        return Task.FromResult(result);
    }

    public async Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
        {
            return null;
        }

        var latestByJobKey = BuildLatestByJobKey(jobName);
        if (!_memoryCache.TryGetValue(latestByJobKey, out string? jobExecutionId)
            || string.IsNullOrWhiteSpace(jobExecutionId))
        {
            return null;
        }

        return await GetByJobExecutionIdAsync(jobExecutionId, cancellationToken);
    }

    private static string BuildAttemptKey(string jobExecutionId)
        => AttemptPrefix + jobExecutionId.Trim().ToLowerInvariant();

    private static string BuildLatestByJobKey(string jobName)
        => LatestByJobPrefix + jobName.Trim().ToLowerInvariant();
}
