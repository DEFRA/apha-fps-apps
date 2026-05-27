using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailNotificationServiceTests
{
    [Fact]
    public async Task SendFailureNotificationAsync_WhenNotificationsDisabled_ShouldReturnWithoutThrowing()
    {
        var settings = Options.Create(new MabArchiveSettings
        {
            EnableEmailNotifications = false,
            AdminNotificationEmail = "alerts@example.com"
        });

        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings);

        await service.SendFailureNotificationAsync("cid-1", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenAdminEmailMissing_ShouldReturnWithoutThrowing()
    {
        var settings = Options.Create(new MabArchiveSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "   "
        });

        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings);

        await service.SendFailureNotificationAsync("cid-2", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenEnabledAndConfigured_ShouldPrepareNotificationWithoutThrowing()
    {
        var settings = Options.Create(new MabArchiveSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "alerts@example.com",
            CloudWatchLogGroup = "batchjobs-log-group"
        });

        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings);

        await service.SendFailureNotificationAsync("cid-3", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }
}