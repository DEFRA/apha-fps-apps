namespace Apha.BatchJobs.Application.Configuration;

/// <summary>
/// Global Worker execution-notification policy — applies automatically to every job run through
/// <c>JobOrchestrator</c>, not just a per-job allow-list (Worker-Wide Batch Execution Notifications
/// spec, section 7). A newly registered job inherits this behaviour with no configuration change.
/// </summary>
public sealed class BatchAlertingSettings
{
    /// <summary>Master switch. When false, no operational execution email is ever sent regardless of <see cref="NotifyOnSuccess"/>/<see cref="NotifyOnFailure"/>.</summary>
    public bool EnableEmailNotifications { get; set; }

    /// <summary>When true (and <see cref="EnableEmailNotifications"/> is true), a job reaching <c>Completed</c> sends a success notification.</summary>
    public bool NotifyOnSuccess { get; set; }

    /// <summary>When true (and <see cref="EnableEmailNotifications"/> is true), a job reaching <c>Failed</c> sends a failure notification.</summary>
    public bool NotifyOnFailure { get; set; }

    /// <summary>Email recipient for operational execution notifications.</summary>
    public string? AdminNotificationEmail { get; set; }
}
