namespace Apha.BatchJobs.Pact.Api.Models;

public sealed class BatchCancelRequest
{
    public string JobExecutionId { get; init; } = string.Empty;

    public string? RequestedBy { get; init; }
}
