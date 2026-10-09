using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Application.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

public sealed class BatchJobSettingsValidatorTests
{
    private readonly BatchJobSettingsValidator _validator = new();

    [Fact]
    public void Validate_LeasesDisabled_Succeeds()
    {
        var result = _validator.Validate(null, new BatchJobSettings { LockTimeoutSeconds = 0, HeartbeatIntervalSeconds = 600, RetryDelaySeconds = 600 });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ShippedAppSettingsValues_Succeeds()
    {
        // 30 + (60 + 29 jitter) + 30 = 149 < 300
        var result = _validator.Validate(null, new BatchJobSettings
        {
            LockTimeoutSeconds = 300, HeartbeatIntervalSeconds = 30, RetryAttempts = 3, RetryDelaySeconds = 60
        });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_RetryGapReachesLease_Fails()
    {
        // 30 + (60 + 29 jitter) + 30 = 149, not below 149
        var result = _validator.Validate(null, new BatchJobSettings
        {
            LockTimeoutSeconds = 149, HeartbeatIntervalSeconds = 30, RetryAttempts = 1, RetryDelaySeconds = 60
        });

        Assert.True(result.Failed);
        Assert.Contains("LockTimeoutSeconds (149)", result.FailureMessage);
        Assert.Contains("149s", result.FailureMessage);
    }

    [Fact]
    public void Validate_RetryGapJustBelowLease_Succeeds()
    {
        var result = _validator.Validate(null, new BatchJobSettings
        {
            LockTimeoutSeconds = 150, HeartbeatIntervalSeconds = 30, RetryAttempts = 1, RetryDelaySeconds = 60
        });

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_NoRetries_OnlyHeartbeatMustBeBelowLease()
    {
        var settings = new BatchJobSettings { LockTimeoutSeconds = 60, HeartbeatIntervalSeconds = 30, RetryAttempts = 0, RetryDelaySeconds = 600 };

        Assert.True(_validator.Validate(null, settings).Succeeded);

        settings.HeartbeatIntervalSeconds = 60;
        Assert.True(_validator.Validate(null, settings).Failed);
    }

    [Fact]
    public void AddBatchApplicationServices_InvalidLockSettings_FailOnOptionsAccess()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BatchJobs:LockTimeoutSeconds"] = "60",
                ["BatchJobs:HeartbeatIntervalSeconds"] = "30",
                ["BatchJobs:RetryAttempts"] = "3",
                ["BatchJobs:RetryDelaySeconds"] = "60"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddBatchApplicationServices(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<BatchJobSettings>>().Value);
    }
}
