using System.ComponentModel.DataAnnotations;

namespace Apha.BatchJobs.Pact.Api.Models;

public sealed class BatchCancelRequest
{
    [Required]
    public string JobExecutionId { get; init; } = string.Empty;

    [Required]
    public string RequestedBy { get; init; } = string.Empty;
}
