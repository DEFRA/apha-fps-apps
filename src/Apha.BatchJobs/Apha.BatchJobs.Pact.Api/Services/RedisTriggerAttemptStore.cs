using System.Text.Json;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class RedisTriggerAttemptStore : ITriggerAttemptStore
{
    private const string AttemptPrefix = "pact-trigger-attempt:";
    private const string LatestByJobPrefix = "pact-trigger-latest-job:";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisTriggerAttemptStore> _logger;
    private readonly TimeSpan _entryTtl;

    public RedisTriggerAttemptStore(
        IDistributedCache cache,
        IOptions<TriggerStoreOptions> options,
        ILogger<RedisTriggerAttemptStore> logger)
    {
        _cache = cache;
        _logger = logger;

        var configuredTtlMinutes = options.Value.EntryTtlMinutes;
        _entryTtl = TimeSpan.FromMinutes(configuredTtlMinutes > 0 ? configuredTtlMinutes : 60);
    }

    public string StoreName => "PactRedisCache";

    public async Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default)
    {
        var attemptKey = BuildAttemptKey(record.JobExecutionId);
        var latestByJobKey = BuildLatestByJobKey(record.JobName);

        var entryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _entryTtl
        };

        var serializedRecord = JsonSerializer.Serialize(record, JsonOptions);
        await _cache.SetStringAsync(attemptKey, serializedRecord, entryOptions, cancellationToken);
        await _cache.SetStringAsync(latestByJobKey, record.JobExecutionId, entryOptions, cancellationToken);
    }

    public async Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobExecutionId))
        {
            return null;
        }

        var attemptKey = BuildAttemptKey(jobExecutionId);
        var serialized = await _cache.GetStringAsync(attemptKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TriggerAttemptRecord>(serialized, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to deserialize trigger attempt record from distributed cache for JobExecutionId={JobExecutionId}",
                jobExecutionId);
            return null;
        }
    }

    public async Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
        {
            return null;
        }

        var latestByJobKey = BuildLatestByJobKey(jobName);
        var jobExecutionId = await _cache.GetStringAsync(latestByJobKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(jobExecutionId))
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
