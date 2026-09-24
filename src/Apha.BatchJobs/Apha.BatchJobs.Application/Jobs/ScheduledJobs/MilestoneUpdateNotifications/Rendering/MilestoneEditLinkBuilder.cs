using Apha.BatchJobs.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <inheritdoc cref="IMilestoneEditLinkBuilder"/>
public sealed class MilestoneEditLinkBuilder : IMilestoneEditLinkBuilder
{
    private readonly MilestoneNotificationsSettings _settings;

    public MilestoneEditLinkBuilder(IOptions<MilestoneNotificationsSettings> settings)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <inheritdoc />
    public string Build(string parentProject)
    {
        if (string.IsNullOrWhiteSpace(parentProject))
            throw new ArgumentException("ParentProject must not be null or whitespace.", nameof(parentProject));

        // ApplicationBaseUrl is validated by NotificationSettingsPreflight, not re-checked here.
        return _settings.ApplicationBaseUrl + Uri.EscapeDataString(parentProject);
    }
}
