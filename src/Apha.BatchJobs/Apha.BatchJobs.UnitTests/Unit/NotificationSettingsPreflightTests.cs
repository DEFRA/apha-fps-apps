using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class NotificationSettingsPreflightTests
{
    private static readonly IOptions<MilestoneNotificationsSettings> DefaultSettings =
        Options.Create(new MilestoneNotificationsSettings { CapsMailbox = "caps@example.com" });

    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new NotificationSettingsPreflight(null!, DefaultSettings, NullLogger<NotificationSettingsPreflight>.Instance));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateInMemoryDbContext();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new NotificationSettingsPreflight(context, null!, NullLogger<NotificationSettingsPreflight>.Instance));

        Assert.Equal("settings", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateInMemoryDbContext();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new NotificationSettingsPreflight(context, DefaultSettings, null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task ValidateAsync_WhenAllRequiredSettingsPresentAndNonBlank_ShouldNotThrow()
    {
        await using var context = CreateInMemoryDbContext();
        SeedSetting(context, "PIMS_Project_Report_Name", " Milestone Report ");
        SeedSetting(context, "PIMS_Project_Current_Root", "https://pims.example.com/projects");
        SeedSetting(context, "PIMS_Project_Edit_Link", "https://pims.example.com/edit");
        SeedSetting(context, "SomeUnrelatedSetting", "irrelevant");
        await context.SaveChangesAsync();

        var preflight = new NotificationSettingsPreflight(context, DefaultSettings, NullLogger<NotificationSettingsPreflight>.Instance);

        await preflight.ValidateAsync(CancellationToken.None);
    }

    [Theory]
    [InlineData("PIMS_Project_Report_Name")]
    [InlineData("PIMS_Project_Current_Root")]
    [InlineData("PIMS_Project_Edit_Link")]
    public async Task ValidateAsync_WhenOneRequiredSettingMissing_ShouldThrowNotificationSettingsConfigurationException(string missingId)
    {
        await using var context = CreateInMemoryDbContext();

        foreach (var id in new[] { "PIMS_Project_Report_Name", "PIMS_Project_Current_Root", "PIMS_Project_Edit_Link" })
        {
            if (id != missingId)
                SeedSetting(context, id, "some-value");
        }
        await context.SaveChangesAsync();

        var preflight = new NotificationSettingsPreflight(context, DefaultSettings, NullLogger<NotificationSettingsPreflight>.Instance);

        var ex = await Assert.ThrowsAsync<NotificationSettingsConfigurationException>(() => preflight.ValidateAsync(CancellationToken.None));
        Assert.Contains(missingId, ex.Message);
        Assert.Contains("missing", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenRequiredSettingValueIsBlank_ShouldThrowNotificationSettingsConfigurationException(string? blankValue)
    {
        await using var context = CreateInMemoryDbContext();
        SeedSetting(context, "PIMS_Project_Report_Name", blankValue);
        SeedSetting(context, "PIMS_Project_Current_Root", "https://pims.example.com/projects");
        SeedSetting(context, "PIMS_Project_Edit_Link", "https://pims.example.com/edit");
        await context.SaveChangesAsync();

        var preflight = new NotificationSettingsPreflight(context, DefaultSettings, NullLogger<NotificationSettingsPreflight>.Instance);

        var ex = await Assert.ThrowsAsync<NotificationSettingsConfigurationException>(() => preflight.ValidateAsync(CancellationToken.None));
        Assert.Contains("PIMS_Project_Report_Name", ex.Message);
        Assert.Contains("blank", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenAllThreeRequiredSettingsMissing_ShouldReportAllThreeInMessage()
    {
        await using var context = CreateInMemoryDbContext();

        var preflight = new NotificationSettingsPreflight(context, DefaultSettings, NullLogger<NotificationSettingsPreflight>.Instance);

        var ex = await Assert.ThrowsAsync<NotificationSettingsConfigurationException>(() => preflight.ValidateAsync(CancellationToken.None));
        Assert.Contains("PIMS_Project_Report_Name", ex.Message);
        Assert.Contains("PIMS_Project_Current_Root", ex.Message);
        Assert.Contains("PIMS_Project_Edit_Link", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WhenCapsMailboxNotConfigured_ShouldThrowNotificationSettingsConfigurationException(string? blankMailbox)
    {
        await using var context = CreateInMemoryDbContext();
        SeedSetting(context, "PIMS_Project_Report_Name", "some-value");
        SeedSetting(context, "PIMS_Project_Current_Root", "https://pims.example.com/projects");
        SeedSetting(context, "PIMS_Project_Edit_Link", "https://pims.example.com/edit");
        await context.SaveChangesAsync();

        var settings = Options.Create(new MilestoneNotificationsSettings { CapsMailbox = blankMailbox });
        var preflight = new NotificationSettingsPreflight(context, settings, NullLogger<NotificationSettingsPreflight>.Instance);

        var ex = await Assert.ThrowsAsync<NotificationSettingsConfigurationException>(() => preflight.ValidateAsync(CancellationToken.None));
        Assert.Contains("CapsMailbox", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WhenTblSettingsRowsMissingAndCapsMailboxMissing_ShouldReportTblSettingsProblemsFirst()
    {
        // The tbl_settings checks fail before CapsMailbox is even inspected — confirms the
        // two validation groups don't get merged into one confusing combined message.
        await using var context = CreateInMemoryDbContext();

        var settings = Options.Create(new MilestoneNotificationsSettings { CapsMailbox = null });
        var preflight = new NotificationSettingsPreflight(context, settings, NullLogger<NotificationSettingsPreflight>.Instance);

        var ex = await Assert.ThrowsAsync<NotificationSettingsConfigurationException>(() => preflight.ValidateAsync(CancellationToken.None));
        Assert.Contains("PIMS_Project_Report_Name", ex.Message);
        Assert.DoesNotContain("CapsMailbox", ex.Message);
    }

    private static void SeedSetting(BatchJobsDbContext context, string id, string? setting) =>
        context.MaTblSettings.Add(new MaTblSettings { Id = id, Setting = setting });

    private static BatchJobsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }
}
