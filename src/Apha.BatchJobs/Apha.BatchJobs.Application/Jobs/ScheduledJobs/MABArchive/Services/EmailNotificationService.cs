using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Entities.Email;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

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
            var subject = isSuccess
                ? $"FPS Batch Job Completed Successfully - {notification.JobName}"
                : $"FPS Batch Job Failed - {notification.JobName}";
            var body = BuildExecutionNotificationBody(notification, isSuccess);

            _logger.LogInformation(
                "Sending execution notification | JobExecutionId={JobExecutionId} | Job={JobName} | FinalStatus={FinalStatus} | To={Email}",
                notification.JobExecutionId, notification.JobName, notification.FinalStatus, _settings.AdminNotificationEmail);

            var message = new EmailMessage([_settings.AdminNotificationEmail], subject, body);
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

    private static string BuildExecutionNotificationBody(BatchExecutionNotification notification, bool isSuccess)
    {
        var body = new StringBuilder();

        body.AppendLine(isSuccess
            ? $"The FPS Batch Job '{notification.JobName}' completed successfully."
            : $"The FPS Batch Job '{notification.JobName}' failed.");
        body.AppendLine();
        body.AppendLine($"Job Name: {notification.JobName}");
        body.AppendLine($"Job Execution ID: {notification.JobExecutionId}");
        body.AppendLine($"Job Queue ID: {notification.JobQueueId}");
        body.AppendLine($"Run Mode: {notification.RunMode}");

        if (!string.IsNullOrWhiteSpace(notification.RequestedBy))
        {
            body.AppendLine($"Requested By: {notification.RequestedBy}");
        }

        if (notification.RequestedAtUtc.HasValue)
        {
            body.AppendLine($"Requested At UTC: {notification.RequestedAtUtc:yyyy-MM-dd HH:mm:ss}");
        }

        body.AppendLine($"Final Status: {notification.FinalStatus}");
        body.AppendLine(isSuccess
            ? $"Completed At UTC: {notification.FinishedAtUtc:yyyy-MM-dd HH:mm:ss}"
            : $"Failed At UTC: {notification.FinishedAtUtc:yyyy-MM-dd HH:mm:ss}");

        if (notification.Duration.HasValue)
        {
            body.AppendLine($"Duration: {notification.Duration}");
        }

        if (!isSuccess)
        {
            body.AppendLine();
            body.AppendLine("Failure:");
            body.AppendLine(string.IsNullOrWhiteSpace(notification.FailureMessage)
                ? "(no failure message provided)"
                : notification.FailureMessage);
        }

        body.AppendLine();
        body.AppendLine(isSuccess
            ? "This is an automated notification. Please do not reply to this email."
            : "This is an automated notification. Please review the BatchJobs logs for further details.");

        return body.ToString();
    }
}
