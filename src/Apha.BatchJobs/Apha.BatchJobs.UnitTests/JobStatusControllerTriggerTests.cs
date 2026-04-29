using Apha.BatchJobs.Api.Controllers;
using Apha.BatchJobs.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class JobStatusControllerTriggerTests
{
    private readonly IJobStatusService _statusService = Substitute.For<IJobStatusService>();
    private readonly IJobTriggerService _triggerService = Substitute.For<IJobTriggerService>();
    private readonly ILogger<JobStatusController> _logger = Substitute.For<ILogger<JobStatusController>>();

    [Fact]
    public async Task Trigger_WhenJobCanRun_ReturnsAcceptedAndOperationId()
    {
        // Arrange
        _statusService
            .GetStatusAsync("HealthCheck", Arg.Any<CancellationToken>())
            .Returns(new JobStatusResult
            {
                JobName = "HealthCheck",
                IsRunning = false,
                ActiveLock = null,
                LastExecution = null
            });

        var acceptedAt = DateTime.UtcNow;
        _triggerService
            .TriggerAsync("HealthCheck", Arg.Any<CancellationToken>())
            .Returns(new TriggerResult("op-123", acceptedAt));

        var controller = CreateController();

        // Act
        var actionResult = await controller.Trigger("HealthCheck", CancellationToken.None);

        // Assert
        var accepted = Assert.IsType<AcceptedResult>(actionResult);
        Assert.Equal(202, accepted.StatusCode);
        Assert.True(GetPropertyValue<bool>(accepted.Value, "accepted"));
        Assert.Equal("op-123", GetPropertyValue<string>(accepted.Value, "operationId"));
        Assert.Equal("HealthCheck", GetPropertyValue<string>(accepted.Value, "jobName"));

        await _triggerService.Received(1).TriggerAsync("HealthCheck", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Trigger_WhenJobAlreadyRunning_ReturnsConflictAndDoesNotStartNewRun()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _statusService
            .GetStatusAsync("HealthCheck", Arg.Any<CancellationToken>())
            .Returns(new JobStatusResult
            {
                JobName = "HealthCheck",
                IsRunning = true,
                ActiveLock = new ActiveLockInfo
                {
                    RunId = "run-1",
                    AcquiredAt = now.AddMinutes(-1),
                    ExpiresAt = now.AddMinutes(30)
                },
                LastExecution = null
            });

        var controller = CreateController();

        // Act
        var actionResult = await controller.Trigger("HealthCheck", CancellationToken.None);

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(actionResult);
        Assert.Equal(409, conflict.StatusCode);
        Assert.False(GetPropertyValue<bool>(conflict.Value, "accepted"));
        Assert.Equal("HealthCheck", GetPropertyValue<string>(conflict.Value, "jobName"));
        Assert.Equal("run-1", GetPropertyValue<string>(conflict.Value, "runId"));

        await _triggerService.DidNotReceive().TriggerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Trigger_WhenJobNotRegistered_ReturnsNotFound()
    {
        // Arrange
        _statusService
            .GetStatusAsync("UnknownJob", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<JobStatusResult>(new InvalidOperationException("Unknown job")));

        var controller = CreateController();

        // Act
        var actionResult = await controller.Trigger("UnknownJob", CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(actionResult);
        Assert.Equal(404, notFound.StatusCode);
        Assert.Equal("UnknownJob", GetPropertyValue<string>(notFound.Value, "jobName"));

        await _triggerService.DidNotReceive().TriggerAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private JobStatusController CreateController()
        => new(_statusService, _triggerService, _logger);

    private static T GetPropertyValue<T>(object? source, string propertyName)
    {
        Assert.NotNull(source);

        var prop = source.GetType().GetProperty(propertyName);
        Assert.NotNull(prop);

        var value = prop.GetValue(source);
        Assert.NotNull(value);

        return Assert.IsType<T>(value);
    }
}
