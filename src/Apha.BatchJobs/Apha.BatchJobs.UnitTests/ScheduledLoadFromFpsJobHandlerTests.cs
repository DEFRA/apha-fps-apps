using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class ScheduledLoadFromFpsJobHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_RecordsRunAndStepAudit_ForSuccessfulPlan()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlationService = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();
        var stepHandler = Substitute.For<IScheduledLoadFromFpsStepHandler>();

        var context = new ScheduledLoadFromFpsExecutionContext(3, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(
            context,
            new[] { ScheduledLoadFromFpsStep.ProcessPreviousYearTotals }));

        var runId = Guid.NewGuid();
        var stepRunId = Guid.NewGuid();

        correlationService.GetCorrelationId().Returns((string?)null);
        correlationService.GenerateCorrelationId().Returns("corr-123");
        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-123", Arg.Any<CancellationToken>()).Returns(runId);
        repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessPreviousYearTotals, 1, Arg.Any<CancellationToken>()).Returns(stepRunId);

        stepHandler.Step.Returns(ScheduledLoadFromFpsStep.ProcessPreviousYearTotals);
        stepHandler.ExecuteAsync(context, Arg.Any<CancellationToken>()).Returns(12);
        validation.ExecuteAsync(runId, context, 1, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ScheduledLoadValidationAssertionResult>());

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlationService,
            validation,
            new[] { stepHandler },
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await sut.ExecuteAsync(CancellationToken.None);

        await repository.Received(1).CompleteStepAsync(stepRunId, "Completed", 12, null, Arg.Any<CancellationToken>());
        await repository.Received(1).CompleteRunAsync(runId, "Completed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenStepFails_MarksStepAndRunFailed()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlationService = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();
        var stepHandler = Substitute.For<IScheduledLoadFromFpsStepHandler>();

        var context = new ScheduledLoadFromFpsExecutionContext(3, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(
            context,
            new[] { ScheduledLoadFromFpsStep.ProcessPreviousYearTotals }));

        var runId = Guid.NewGuid();
        var stepRunId = Guid.NewGuid();
        var failure = new InvalidOperationException("step failed");

        correlationService.GetCorrelationId().Returns("corr-456");
        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-456", Arg.Any<CancellationToken>()).Returns(runId);
        repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessPreviousYearTotals, 1, Arg.Any<CancellationToken>()).Returns(stepRunId);

        stepHandler.Step.Returns(ScheduledLoadFromFpsStep.ProcessPreviousYearTotals);
        stepHandler.ExecuteAsync(context, Arg.Any<CancellationToken>()).Returns(_ => throw failure);

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlationService,
            validation,
            new[] { stepHandler },
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(CancellationToken.None));

        await repository.Received(1).CompleteStepAsync(
            stepRunId,
            "Failed",
            null,
            "step failed",
            Arg.Any<CancellationToken>());
        await repository.Received(1).CompleteRunAsync(runId, "Failed", Arg.Any<CancellationToken>());
    }
}
