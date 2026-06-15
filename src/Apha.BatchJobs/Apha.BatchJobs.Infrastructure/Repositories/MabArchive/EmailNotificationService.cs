using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apha.BatchJobs.Domain.Configuration;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IEmailNotificationService.
/// Sends email notifications on job failure.
/// Currently a stub; real SMTP integration to be implemented in next phase.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly MabArchiveSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="settings">MABArchive settings options.</param>
    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Sends a failure notification for a MABArchive execution.
    /// </summary>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="jobName">Job name.</param>
    /// <param name="errorMessage">Error message text.</param>
    /// <param name="timestamp">Failure timestamp (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendFailureNotificationAsync(
        string correlationId,
        string jobName,
        string errorMessage,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        if (!_settings.EnableEmailNotifications)
        {
            _logger.LogInformation("Email notifications disabled. Skipping failure notification for CorrelationId={CorrelationId}", correlationId);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.AdminNotificationEmail))
        {
            _logger.LogWarning("AdminNotificationEmail not configured. Cannot send failure notification for CorrelationId={CorrelationId}", correlationId);
            return;
        }

        try
        {
            _logger.LogInformation(
                "Sending failure notification | CorrelationId={CorrelationId} | Job={JobName} | To={Email}",
                correlationId,
                jobName,
                _settings.AdminNotificationEmail);

            var subject = $"[ALERT] {jobName} Job Failed - {timestamp:yyyy-MM-dd HH:mm:ss}";
            var body = $@"
Job Failure Notification

Job Name: {jobName}
Correlation ID: {correlationId}
Failure Time: {timestamp:yyyy-MM-dd HH:mm:ss} UTC
Error Message: {errorMessage}

For detailed diagnostics, check:
- CloudWatch Logs (group: {_settings.CloudWatchLogGroup}) filtered by CorrelationId={correlationId}
- Database lock table (tbl_job_queue) for lock status
- Recent batch job execution records

Contact your system administrator for assistance.
";

            // TODO: Implement SMTP email sending in next phase
            // For now, log the notification details
            _logger.LogInformation("Failure notification prepared | Subject={Subject} | To={Email}", subject, _settings.AdminNotificationEmail);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send failure notification for CorrelationId={CorrelationId}", correlationId);
            throw;
        }
    }
}
