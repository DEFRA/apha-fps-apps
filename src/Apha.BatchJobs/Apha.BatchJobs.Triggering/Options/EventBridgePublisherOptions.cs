namespace Apha.BatchJobs.Triggering.Options;

public sealed class EventBridgePublisherOptions
{
    public string EventBusName { get; set; } = "default";

    public string Source { get; set; } = "batchjobs.api";

    public string DetailType { get; set; } = "BatchJobTriggerRequested";

    public bool DryRun { get; set; } = true;
}