using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <summary>Renders the manager notification email from spec section 13's suggested template. Pure — takes already-grouped/classified data, does no I/O.</summary>
public interface IEmailTemplateRenderer
{
    /// <summary>Fixed subject constant — not configurable, for legacy logic parity.</summary>
    string Subject { get; }

    /// <summary>Renders one manager's email body. Projects with a missing/blank ParentProject are dropped from <see cref="EmailTemplateRenderResult.IncludedProjects"/> and surfaced instead in <see cref="EmailTemplateRenderResult.ExcludedProjects"/>, since a valid milestone edit URL cannot be generated for them.</summary>
    /// <param name="managerName">Recipient display name, substituted into {{ManagerName}}.</param>
    /// <param name="projects">Deduplicated, code-ordered project links for this recipient group.</param>
    /// <param name="includeConfirmationInstruction">
    /// Whether the calendar month is one of the configured mandatory-confirmation months
    /// (spec section 12) — the caller decides this, the renderer stays month-agnostic.
    /// </param>
    EmailTemplateRenderResult RenderManagerEmailBody(
        string managerName,
        IReadOnlyList<NotificationProjectLink> projects,
        bool includeConfirmationInstruction);
}
