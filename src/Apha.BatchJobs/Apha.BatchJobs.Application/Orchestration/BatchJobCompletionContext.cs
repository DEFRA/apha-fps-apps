using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Application.Orchestration;

/// <summary>
/// Immutable snapshot of the finished job's identity and outcome, passed to post-completion hooks.
/// </summary>
public sealed record BatchJobCompletionContext(
    Guid JobQueueId,
    Guid JobExecutionId,
    string JobName,
    int? FpsYear,
    string RequestedBy,
    JobStatus Status,
    string? ErrorMessage);
