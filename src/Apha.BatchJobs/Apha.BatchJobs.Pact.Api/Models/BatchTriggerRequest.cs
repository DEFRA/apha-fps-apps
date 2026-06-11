using System.ComponentModel.DataAnnotations;

namespace Apha.BatchJobs.Pact.Api.Models;

public sealed class BatchTriggerRequest
{
    [Required]
    public string JobName { get; init; } = string.Empty;

    [Required]
    public string RequestedBy { get; init; } = string.Empty;

    public string? ParametersJson { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }
}
