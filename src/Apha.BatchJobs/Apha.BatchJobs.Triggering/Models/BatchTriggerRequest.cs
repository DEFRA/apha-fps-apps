namespace Apha.BatchJobs.Triggering.Models;

public sealed class BatchTriggerRequest
{
    public string JobName { get; init; } = string.Empty;

    public string RequestedBy { get; init; } = string.Empty;
}