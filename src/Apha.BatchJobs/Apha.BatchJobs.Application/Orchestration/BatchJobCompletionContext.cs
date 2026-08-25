namespace Apha.BatchJobs.Application.Orchestration;

/// <summary>
/// Immutable snapshot of the completed job's identity, passed to post-completion hooks.
/// </summary>
public sealed record BatchJobCompletionContext(
    Guid JobQueueId,
    Guid JobExecutionId,
    string JobName,
    int? FpsYear,
    string RequestedBy);
