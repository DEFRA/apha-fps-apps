namespace Apha.BatchJobs.Pact.Api.Options;

public sealed class TriggerStoreOptions
{
    public string Provider { get; init; } = "Memory";

    public int EntryTtlMinutes { get; init; } = 60;

    public string? RedisConnectionString { get; init; }

    public string RedisInstanceName { get; init; } = "pact-trigger-store:";
}
