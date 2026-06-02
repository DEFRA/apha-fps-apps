namespace Apha.BatchJobs.Pact.Api.Options;

public sealed class EventPublisherOptions
{
    public string EventBusName { get; init; } = "default";

    public string Source { get; init; } = "pact.api";

    public string DetailType { get; init; } = "BatchJob.TriggerRequested";

    public bool DryRun { get; init; } = true;
}
