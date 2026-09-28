using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Domain.Entities.Email;

/// <summary>
/// Execution metadata for a job's terminal outcome (Completed or Failed), passed to
/// <c>IEmailNotificationService.SendExecutionNotificationAsync</c>. Deciding whether to send is
/// the caller's responsibility; this record only carries what the notification content needs.
/// </summary>
public sealed record BatchExecutionNotification(
    string JobName,
    Guid JobExecutionId,
    Guid JobQueueId,
    RunMode RunMode,
    string? RequestedBy,
    DateTime? RequestedAtUtc,
    JobStatus FinalStatus,
    DateTime FinishedAtUtc,
    TimeSpan? Duration,
    string? FailureMessage);
