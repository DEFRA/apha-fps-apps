using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Sends operational execution-notification emails through <see cref="IEmailService"/>.
/// Resolves <see cref="IEmailService"/> lazily: eager resolution would throw wherever
/// GraphEmailSettings is unconfigured, breaking every job that depends on this service.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly BatchAlertingSettings _settings;
    private readonly Func<IEmailService> _emailServiceFactory;

    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IOptions<BatchAlertingSettings> settings,
        Func<IEmailService> emailServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new BatchAlertingSettings();
        _emailServiceFactory = emailServiceFactory ?? throw new ArgumentNullException(nameof(emailServiceFactory));
    }

    /// <inheritdoc />
    public async Task SendExecutionNotificationAsync(BatchExecutionNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (string.IsNullOrWhiteSpace(_settings.AdminNotificationEmail))
        {
            _logger.LogWarning(
                "AdminNotificationEmail not configured. Cannot send execution notification for JobExecutionId={JobExecutionId}",
                notification.JobExecutionId);
            return;
        }

        try
        {
            var isSuccess = notification.FinalStatus == JobStatus.Completed;
            var displayName = BatchJobDisplayNames.GetDisplayName(notification.JobName);
            var subject = isSuccess
                ? $"FPS Batch Job Completed Successfully – {displayName}"
                : $"FPS Batch Job Failed – {displayName}";
            var body = BuildExecutionNotificationBody(displayName, isSuccess);

            _logger.LogInformation(
                "Sending execution notification | JobExecutionId={JobExecutionId} | Job={JobName} | FinalStatus={FinalStatus} | To={Email}",
                notification.JobExecutionId, notification.JobName, notification.FinalStatus, _settings.AdminNotificationEmail);

            var message = new EmailMessage([_settings.AdminNotificationEmail], subject, body, IsBodyHtml: false);
            var result = await _emailServiceFactory().SendAsync(message, cancellationToken);

            if (result.Succeeded)
            {
                _logger.LogInformation("Execution notification sent | Subject={Subject} | To={Email}", subject, _settings.AdminNotificationEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Execution notification could not be sent | Subject={Subject} | To={Email} | Reason={Reason}",
                    subject, _settings.AdminNotificationEmail, result.FailureMessage);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Best-effort: a notification delivery failure must never change or mask the
            // already-persisted job outcome, so this is logged and swallowed, not rethrown.
            _logger.LogError(ex, "Failed to send execution notification for JobExecutionId={JobExecutionId}", notification.JobExecutionId);
        }
    }

    private static string BuildExecutionNotificationBody(string displayName, bool isSuccess) =>
        isSuccess
            ? $"The {displayName} process has completed successfully.\n\nThank you for your support."
            : $"The {displayName} process did not complete successfully.\n\nPlease review the details and take necessary action.\n\nThank you for your support.";
}
