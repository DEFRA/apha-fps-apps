namespace Apha.BatchJobs.Fps.Api.Models;

public sealed record BatchTriggerEventDetail(
    string JobExecutionId,
    string JobName,
    string RunMode,
    string RequestedBy,
    DateTime RequestedAtUtc);
