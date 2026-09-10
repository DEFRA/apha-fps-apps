namespace Apha.BatchJobs.Application.Configuration;

/// <summary>
/// Configuration settings for the MilestoneUpdateNotifications scheduled job's email
/// delivery (plan section 10). Extended by later build steps as they need more of the
/// spec section 11 config block (ApplicationBaseUrl, CapsMailbox, MandatoryConfirmationMonths, etc.).
/// </summary>
public class MilestoneNotificationsSettings
{
    /// <summary>
    /// Approved support contact substituted into {{SupportContact}} in the manager
    /// email template (plan section 10.2).
    /// </summary>
    public string? SupportContact { get; set; }

    /// <summary>
    /// Base URL for the FPS application — used by MilestoneLinkBuilder as a fallback
    /// path when the view's EditLink value is unavailable (plan section 9.2).
    /// Must begin with "https://" — validated at job start (plan section 22).
    /// Inert when the EditLink path is in use.
    /// </summary>
    public string? ApplicationBaseUrl { get; set; }

    /// <summary>
    /// CAPS team mailbox address — recipient for the run-summary email (plan section 11.4, §13).
    /// Required for a live send; placeholder until stakeholder confirms the address.
    /// </summary>
    public string? CapsMailbox { get; set; }

    /// <summary>
    /// Calendar month numbers (1–12) for which the email body must include the
    /// confirmation instruction paragraph (plan section 6.2, spec section 12).
    /// Empty list = confirmation instruction never included.
    /// </summary>
    public List<int> MandatoryConfirmationMonths { get; set; } = [];

    /// <summary>
    /// When true, allows a <c>monthOverride</c> parameter to be supplied even when
    /// <c>ASPNETCORE_ENVIRONMENT</c> is "Production". Must be an explicit, deliberate
    /// opt-in — not set by default. Guards against accidental production reruns with
    /// a misleading audit month (plan section 6.2).
    /// </summary>
    public bool AllowMonthOverrideInProduction { get; set; } = false;

    /// <summary>
    /// Temporary DEV/test switch. When true, real per-manager DB-resolved recipients are never
    /// emailed — at most one email for the whole execution goes to <see cref="OverrideRecipient"/>
    /// instead, so the job's real recipient count can't determine how many test emails land in
    /// one inbox. Must default to false; Production must never set this true.
    /// </summary>
    public bool OverrideRecipientEnabled { get; set; } = false;

    /// <summary>
    /// The single address that receives the one allowed test email when
    /// <see cref="OverrideRecipientEnabled"/> is true. Required whenever the override is active —
    /// an empty value fails the send rather than risk falling through to a real recipient.
    /// </summary>
    public string? OverrideRecipient { get; set; }
}
