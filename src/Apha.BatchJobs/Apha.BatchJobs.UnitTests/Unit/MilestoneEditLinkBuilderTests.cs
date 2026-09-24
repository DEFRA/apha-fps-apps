using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class MilestoneEditLinkBuilderTests
{
    private static MilestoneEditLinkBuilder CreateBuilder(string? applicationBaseUrl) =>
        new(Options.Create(new MilestoneNotificationsSettings { ApplicationBaseUrl = applicationBaseUrl }));

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MilestoneEditLinkBuilder(null!));
        Assert.Equal("settings", ex.ParamName);
    }

    [Fact]
    public void Build_WithValidBaseUrlAndParentProject_ShouldReturnCombinedUrl()
    {
        var builder = CreateBuilder("https://fps-apps-dev.aws-int.defra.cloud/PIMS/PMDMilestone?parentproject=");

        var result = builder.Build("ABC123");

        Assert.Equal(
            "https://fps-apps-dev.aws-int.defra.cloud/PIMS/PMDMilestone?parentproject=ABC123",
            result);
    }

    [Theory]
    [InlineData("ABC 123", "ABC%20123")]
    [InlineData("ABC&123", "ABC%26123")]
    [InlineData("ABC/123", "ABC%2F123")]
    [InlineData("ABC?123", "ABC%3F123")]
    public void Build_WhenParentProjectContainsSpecialCharacters_ShouldUrlEncodeIt(string parentProject, string encoded)
    {
        var builder = CreateBuilder("https://fps-dev.example.com/PIMS/PMDMilestone?parentproject=");

        var result = builder.Build(parentProject);

        Assert.Equal($"https://fps-dev.example.com/PIMS/PMDMilestone?parentproject={encoded}", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_WhenParentProjectIsBlank_ShouldThrowArgumentException(string? parentProject)
    {
        var builder = CreateBuilder("https://fps-dev.example.com/PIMS/PMDMilestone?parentproject=");

        var ex = Assert.Throws<ArgumentException>(() => builder.Build(parentProject!));
        Assert.Equal("parentProject", ex.ParamName);
    }
}
