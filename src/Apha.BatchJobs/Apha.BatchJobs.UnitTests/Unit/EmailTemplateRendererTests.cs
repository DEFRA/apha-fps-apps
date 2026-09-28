using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailTemplateRendererTests
{
    private const string ApplicationBaseUrl = "https://fps-apps-dev.aws-int.defra.cloud/PIMS/PMDMilestone?parentproject=";

    private static EmailTemplateRenderer CreateRenderer(string? applicationBaseUrl = ApplicationBaseUrl)
    {
        var settings = Options.Create(new MilestoneNotificationsSettings
        {
            SupportContact = "support@example.com",
            ApplicationBaseUrl = applicationBaseUrl
        });
        return new EmailTemplateRenderer(settings, new MilestoneEditLinkBuilder(settings));
    }

    private readonly EmailTemplateRenderer _renderer = CreateRenderer();

    [Fact]
    public void Subject_ShouldBeFixedConstant()
    {
        Assert.Equal("Milestone and Deliverable Update Request", _renderer.Subject);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldSubstituteManagerNameAndSupportContact()
    {
        var result = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("Dear Jane Smith,", result.HtmlBody);
        Assert.Contains("support@example.com", result.HtmlBody);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldHtmlEncodeManagerName()
    {
        var result = _renderer.RenderManagerEmailBody("Jane <script>alert(1)</script>", [], includeConfirmationInstruction: false);

        Assert.DoesNotContain("<script>", result.HtmlBody);
        Assert.Contains("&lt;script&gt;", result.HtmlBody);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenProjectHasParentProject_ShouldIncludeBuilderGeneratedLink()
    {
        var projects = new[] { new NotificationProjectLink(2026, "PROJ-A", EditLink: null) };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Single(result.IncludedProjects);
        Assert.Empty(result.ExcludedProjects);
        Assert.Contains($"<a href=\"{ApplicationBaseUrl}PROJ-A\">PROJ-A</a>", result.HtmlBody);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RenderManagerEmailBody_WhenParentProjectBlank_ShouldExcludeProject_NotThrow(string? parentProject)
    {
        var projects = new[] { new NotificationProjectLink(2026, parentProject!, EditLink: null) };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Empty(result.IncludedProjects);
        Assert.Single(result.ExcludedProjects);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenMixOfValidAndBlankParentProjects_ShouldPartitionCorrectly()
    {
        var projects = new[]
        {
            new NotificationProjectLink(2026, "PROJ-A", EditLink: null),
            new NotificationProjectLink(2026, "", EditLink: null),
        };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Single(result.IncludedProjects);
        Assert.Equal("PROJ-A", result.IncludedProjects[0].ParentProject);
        Assert.Single(result.ExcludedProjects);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldUseApplicationBaseUrl_NotLegacyEditLink()
    {
        // Source-of-truth regression test: the legacy view's EditLink must never reach the PM
        // email, even when present — only ApplicationBaseUrl + ParentProject is authoritative.
        var renderer = CreateRenderer("https://new-fps/PIMS/PMDMilestone?parentproject=");
        var projects = new[]
        {
            new NotificationProjectLink(2026, "ABC123", "<a href=\"https://legacy-system/old-link/XYZ\">Edit</a>")
        };

        var result = renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Contains("https://new-fps/PIMS/PMDMilestone?parentproject=ABC123", result.HtmlBody);
        Assert.DoesNotContain("https://legacy-system/old-link/XYZ", result.HtmlBody);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenConfirmationInstructionRequested_ShouldIncludeWording()
    {
        var withInstruction = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: true);
        var withoutInstruction = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("confirm", withInstruction.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("confirm your milestone", withoutInstruction.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldIncludeDeputyAndSystemGeneratedWording()
    {
        var result = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("deputy", result.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("system-generated", result.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }
}
