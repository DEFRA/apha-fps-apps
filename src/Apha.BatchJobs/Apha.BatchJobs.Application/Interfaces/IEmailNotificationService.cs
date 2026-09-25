using Apha.BatchJobs.Domain.Entities.Email;

namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Service for sending operational execution-notification emails. Job-agnostic — callers pass
/// the job's name/metadata as parameters rather than this being scoped to any one job.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Sends a Completed or Failed execution notification. The caller decides whether a
    /// notification should be sent at all (master switch, per-outcome policy); this method only
    /// handles delivery — missing-recipient check, subject/body construction, sending, and
    /// logging the outcome. Never throws except <see cref="OperationCanceledException"/>: a
    /// delivery failure must not change or mask the job's already-persisted final status.
    /// </summary>
    /// <param name="notification">Execution metadata for the terminal outcome being reported.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendExecutionNotificationAsync(BatchExecutionNotification notification, CancellationToken cancellationToken);
}
