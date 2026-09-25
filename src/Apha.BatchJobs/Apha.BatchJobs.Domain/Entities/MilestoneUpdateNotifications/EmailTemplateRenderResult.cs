namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>Result of rendering one manager's email body. <see cref="ExcludedProjects"/> carries forward projects with no usable ParentProject so the caller can record them as <c>Skipped</c>/<c>NoValidProjectLinks</c> without re-deriving link validity itself (plan section 10.3, section 11.3).</summary>
public sealed record EmailTemplateRenderResult(
    string HtmlBody,
    IReadOnlyList<NotificationProjectLink> IncludedProjects,
    IReadOnlyList<NotificationProjectLink> ExcludedProjects);
