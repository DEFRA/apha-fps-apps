using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailNotificationServiceTests
{
    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new EmailNotificationService(
                null!,
                Options.Create(new MabArchiveSettings())));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task SendFailureNotificationAsync_WhenSettingsAreNull_ShouldUseDefaultsAndReturn()
    {
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance, settings: null!);

        await service.SendFailureNotificationAsync("cid-null-settings", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None);
    }

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

    [Fact]
    public async Task SendFailureNotificationAsync_WhenLoggerThrowsInTryBlock_ShouldRethrow()
    {
        var settings = Options.Create(new MabArchiveSettings
        {
            EnableEmailNotifications = true,
            AdminNotificationEmail = "alerts@example.com",
            CloudWatchLogGroup = "batchjobs-log-group"
        });

        var service = new EmailNotificationService(new ThrowOnInfoLogger(), settings);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendFailureNotificationAsync("cid-logger-throw", "MABArchive", "boom", DateTime.UtcNow, CancellationToken.None));
    }

    private sealed class ThrowOnInfoLogger : ILogger<EmailNotificationService>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                throw new InvalidOperationException("logger write failed");
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
