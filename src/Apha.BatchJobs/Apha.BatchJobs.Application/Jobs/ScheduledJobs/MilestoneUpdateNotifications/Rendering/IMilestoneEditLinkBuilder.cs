namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <summary>Builds the PM milestone edit link from ApplicationBaseUrl + ParentProject.</summary>
public interface IMilestoneEditLinkBuilder
{
    /// <summary>Throws <see cref="ArgumentException"/> when <paramref name="parentProject"/> is null/empty/whitespace.</summary>
    string Build(string parentProject);
}
