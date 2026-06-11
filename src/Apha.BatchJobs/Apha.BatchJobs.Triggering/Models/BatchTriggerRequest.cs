namespace Apha.BatchJobs.Triggering.Models;

public sealed class BatchTriggerRequest
{
    public string JobName { get; init; } = string.Empty;

    public string RequestedBy { get; init; } = string.Empty;

    public string? ParametersJson { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }
}