namespace Apha.BatchJobs.Pact.Api.Models;

public sealed record BatchTriggerEventDetail(
    string JobExecutionId,
    string JobName,
    string RunMode,
    string RequestedBy,
    DateTime RequestedAtUtc);
