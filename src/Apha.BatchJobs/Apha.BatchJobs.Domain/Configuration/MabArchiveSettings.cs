namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Configuration settings for the MABArchive scheduled job.
/// </summary>
public class MabArchiveSettings
{
    /// <summary>
    /// Lock timeout in seconds. Default: 3600 (1 hour).
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 3600;

    /// <summary>
    /// Database transaction timeout in seconds. Default: 1800 (30 minutes).
    /// </summary>
    public int TransactionTimeoutSeconds { get; set; } = 1800;

    /// <summary>
    /// Email recipient for failure notifications.
    /// </summary>
    public string? AdminNotificationEmail { get; set; }

    /// <summary>
    /// Email sender address.
    /// </summary>
    public string? NotificationFromEmail { get; set; }

    /// <summary>
    /// SMTP server host for email notifications.
    /// </summary>
    public string? SmtpHost { get; set; }

    /// <summary>
    /// SMTP server port for email notifications.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Whether to enable email notifications on failure.
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;

    /// <summary>
    /// CloudWatch log group name for diagnostics.
    /// </summary>
    public string? CloudWatchLogGroup { get; set; }
}
