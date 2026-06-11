namespace Apha.BatchJobs.Triggering.Models;

public sealed record BatchTriggerEventDetail(
    string JobExecutionId,
    string JobName,
    string RunMode,
    string RequestedBy,
    DateTime RequestedAtUtc,
    string? ParametersJson);