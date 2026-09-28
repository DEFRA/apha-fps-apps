namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Validates, before any manager email is sent, that (a) mabarchive.tbl_settings has exactly
/// one non-blank row for each id vprojectreports_pmmail's WHERE clause depends on — a missing
/// row collapses that view to zero rows for every project, silently, indistinguishable from a
/// genuine empty period without this check (plan section 8.1) — and (b)
/// MilestoneNotifications:CapsMailbox is configured, so a missing recipient is caught before
/// manager emails go out rather than discovered only when the CAPS completion email silently
/// fails to send at the end of the run.
/// </summary>
public interface INotificationSettingsPreflight
{
    /// <summary>
    /// Throws <see cref="Apha.BatchJobs.Domain.Exceptions.NotificationSettingsConfigurationException"/>
    /// if any required tbl_settings row is missing, duplicated, or blank, or if CapsMailbox is
    /// not configured.
    /// </summary>
    Task ValidateAsync(CancellationToken cancellationToken);
}
