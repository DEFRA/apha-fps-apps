namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Service for sending email notifications on job failure.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Sends a failure notification email to the configured administrator.
    /// </summary>
    /// <param name="correlationId">The correlation identifier for this execution.</param>
    /// <param name="jobName">The name of the job that failed.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="timestamp">The timestamp when the failure occurred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendFailureNotificationAsync(string correlationId, string jobName, string errorMessage, DateTime timestamp, CancellationToken cancellationToken);
}
