using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Queries;

/// <summary>
/// Implementation of <see cref="INotificationSettingsPreflight"/>. Validates the three
/// mabarchive.tbl_settings ids vprojectreports_pmmail's WHERE clause cross-joins and
/// filters on — a missing/duplicated/blank row collapses that view (and therefore the
/// authoritative candidate query) to zero rows, indistinguishable from a genuine empty
/// period without this explicit upfront check (plan section 8.1) — plus that
/// MilestoneNotifications:CapsMailbox is configured, so a missing recipient fails before
/// manager emails are sent rather than only being discovered when the CAPS completion
/// email silently finalizes as NotAttempted at the end of the run.
/// </summary>
public sealed class NotificationSettingsPreflight : INotificationSettingsPreflight
{
    private static readonly string[] RequiredSettingIds =
    [
        "PIMS_Project_Report_Name",
        "PIMS_Project_Current_Root",
        "PIMS_Project_Edit_Link"
    ];

    /// <summary>Required suffix — MilestoneEditLinkBuilder appends the encoded ParentProject directly onto ApplicationBaseUrl with no separator.</summary>
    private const string RequiredApplicationBaseUrlSuffix = "parentproject=";

    private readonly BatchJobsDbContext _context;
    private readonly MilestoneNotificationsSettings _settings;
    private readonly ILogger<NotificationSettingsPreflight> _logger;

    public NotificationSettingsPreflight(
        BatchJobsDbContext context,
        IOptions<MilestoneNotificationsSettings> settings,
        ILogger<NotificationSettingsPreflight> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.MaTblSettings
            .Where(s => RequiredSettingIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var problems = new List<string>();

        foreach (var id in RequiredSettingIds)
        {
            var matches = rows.Where(r => r.Id == id).ToList();

            if (matches.Count == 0)
                problems.Add($"'{id}' is missing");
            else if (matches.Count > 1)
                problems.Add($"'{id}' has {matches.Count} rows (expected exactly one)");
            else if (string.IsNullOrWhiteSpace(matches[0].Setting))
                problems.Add($"'{id}' is blank");
        }

        if (problems.Count > 0)
        {
            throw new NotificationSettingsConfigurationException(
                "Milestone notification settings preflight failed against mabarchive.tbl_settings: " +
                string.Join("; ", problems) +
                ". vprojectreports_pmmail depends on these rows and would otherwise silently return zero " +
                "candidates for every project.");
        }

        if (string.IsNullOrWhiteSpace(_settings.CapsMailbox))
        {
            throw new NotificationSettingsConfigurationException(
                "Milestone notification settings preflight failed: MilestoneNotifications:CapsMailbox is " +
                "not configured. The run must not send manager emails only to silently skip the CAPS " +
                "completion email at the end — fail before any email goes out instead.");
        }

        ValidateApplicationBaseUrl();

        _logger.LogInformation(
            "Milestone notification settings preflight passed for {SettingCount} required tbl_settings rows, " +
            "CapsMailbox, and ApplicationBaseUrl configuration",
            RequiredSettingIds.Length);
    }

    /// <summary>
    /// Validates MilestoneNotifications:ApplicationBaseUrl — the source of truth for the PM
    /// milestone link (edit-link-configuration spec §8). Must fail before any manager email is
    /// sent rather than let a bad value surface only when a link fails to build per-candidate.
    /// </summary>
    private void ValidateApplicationBaseUrl()
    {
        var baseUrl = _settings.ApplicationBaseUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new NotificationSettingsConfigurationException(
                "Milestone notification settings preflight failed: MilestoneNotifications:ApplicationBaseUrl " +
                "is not configured. This is now the source of truth for the PM milestone link and must be set " +
                "before any manager email is sent.");
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new NotificationSettingsConfigurationException(
                $"Milestone notification settings preflight failed: MilestoneNotifications:ApplicationBaseUrl " +
                $"'{baseUrl}' is not a valid absolute URI.");
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new NotificationSettingsConfigurationException(
                $"Milestone notification settings preflight failed: MilestoneNotifications:ApplicationBaseUrl " +
                $"'{baseUrl}' uses scheme '{uri.Scheme}' — only https is permitted.");
        }

        // A valid https URI alone isn't enough — the builder concatenates onto this directly.
        if (!baseUrl.EndsWith(RequiredApplicationBaseUrlSuffix, StringComparison.Ordinal))
        {
            throw new NotificationSettingsConfigurationException(
                $"Milestone notification settings preflight failed: MilestoneNotifications:ApplicationBaseUrl " +
                $"'{baseUrl}' must end with '{RequiredApplicationBaseUrlSuffix}' — MilestoneEditLinkBuilder " +
                "appends the encoded ParentProject directly onto this value with no separator.");
        }
    }
}
