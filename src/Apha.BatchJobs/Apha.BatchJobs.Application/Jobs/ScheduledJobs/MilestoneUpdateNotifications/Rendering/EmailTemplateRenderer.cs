using System.Net;
using System.Text;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <summary>Implementation of <see cref="IEmailTemplateRenderer"/>, following spec section 13's suggested template. Links come from <see cref="IMilestoneEditLinkBuilder"/> (ApplicationBaseUrl + ParentProject), not the legacy EditLink HTML — projects with no usable ParentProject are reported back via <see cref="EmailTemplateRenderResult.ExcludedProjects"/> rather than silently dropped.</summary>
public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    /// <inheritdoc />
    public string Subject => "Milestone and Deliverable Update Request";

    private readonly MilestoneNotificationsSettings _settings;
    private readonly IMilestoneEditLinkBuilder _linkBuilder;

    public EmailTemplateRenderer(IOptions<MilestoneNotificationsSettings> settings, IMilestoneEditLinkBuilder linkBuilder)
    {
        _settings = settings?.Value ?? new MilestoneNotificationsSettings();
        _linkBuilder = linkBuilder ?? throw new ArgumentNullException(nameof(linkBuilder));
    }

    /// <inheritdoc />
    public EmailTemplateRenderResult RenderManagerEmailBody(
        string managerName,
        IReadOnlyList<NotificationProjectLink> projects,
        bool includeConfirmationInstruction)
    {
        ArgumentNullException.ThrowIfNull(projects);

        var included = new List<NotificationProjectLink>();
        var excluded = new List<NotificationProjectLink>();
        var linksHtml = new StringBuilder();
        linksHtml.Append("<ul>");

        foreach (var project in projects)
        {
            if (string.IsNullOrWhiteSpace(project.ParentProject))
            {
                excluded.Add(project);
                continue;
            }

            included.Add(project);
            linksHtml
                .Append("<li><a href=\"")
                .Append(WebUtility.HtmlEncode(_linkBuilder.Build(project.ParentProject)))
                .Append("\">")
                .Append(WebUtility.HtmlEncode(project.ParentProject))
                .Append("</a></li>");
        }

        linksHtml.Append("</ul>");

        var confirmationInstruction = includeConfirmationInstruction
            ? "<p>Please confirm your milestone or deliverable data due this month, even where no update is made.</p>"
            : string.Empty;

        var body = $"""
            <p>Dear {WebUtility.HtmlEncode(managerName)},</p>

            <p>
            Here are the links to edit the milestones for your projects.
            </p>

            {linksHtml}

            {confirmationInstruction}

            <p>
            If you are not the person named in this email, you are receiving it
            as a deputy for information only. You will not be able to edit these
            milestones or deliverables.
            </p>

            <p>
            This is a system-generated email. Please do not reply.
            For assistance, contact {WebUtility.HtmlEncode(_settings.SupportContact)}.
            </p>
            """;

        return new EmailTemplateRenderResult(body, included, excluded);
    }
}
