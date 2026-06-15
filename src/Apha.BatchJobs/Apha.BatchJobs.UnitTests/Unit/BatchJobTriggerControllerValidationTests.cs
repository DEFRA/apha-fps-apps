using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Pact.Api.Controllers;
using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;

namespace Apha.BatchJobs.UnitTests.Unit;

public sealed class BatchJobTriggerControllerValidationTests
{
    [Fact]
    public async Task Trigger_WhenRequestedByIsNull_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Trigger(
            new BatchTriggerRequest
            {
                JobName = "RecreateSummaries",
                RequestedBy = null!
            },
            CancellationToken.None);

        AssertBadRequestWithRequestedByReason(result);
    }

    [Fact]
    public async Task Trigger_WhenRequestedByIsWhitespace_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Trigger(
            new BatchTriggerRequest
            {
                JobName = "RecreateSummaries",
                RequestedBy = "   "
            },
            CancellationToken.None);

        AssertBadRequestWithRequestedByReason(result);
    }

    [Fact]
    public async Task Cancel_WhenRequestedByIsNull_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Cancel(
            "RecreateSummaries",
            new BatchCancelRequest
            {
                JobExecutionId = Guid.NewGuid().ToString(),
                RequestedBy = null!
            },
            CancellationToken.None);

        AssertBadRequestWithRequestedByReason(result);
    }

    [Fact]
    public async Task Cancel_WhenRequestedByIsWhitespace_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Cancel(
            "RecreateSummaries",
            new BatchCancelRequest
            {
                JobExecutionId = Guid.NewGuid().ToString(),
                RequestedBy = "\t"
            },
            CancellationToken.None);

        AssertBadRequestWithRequestedByReason(result);
    }

    private static BatchJobTriggerController CreateController()
    {
        return new BatchJobTriggerController(
            Substitute.For<ITriggerDispatcher>(),
            Substitute.For<ITriggerAttemptStore>(),
            Substitute.For<IJobExecutionRepository>(),
            Substitute.For<IBatchLockRepository>(),
            NullLogger<BatchJobTriggerController>.Instance,
            Substitute.For<IHostEnvironment>());
    }

    private static void AssertBadRequestWithRequestedByReason(IActionResult actionResult)
    {
        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        var payload = JsonSerializer.Serialize(badRequest.Value);
        Assert.Contains("requestedBy is required.", payload, StringComparison.OrdinalIgnoreCase);
    }
}