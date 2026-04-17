using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests.ScheduledLoadFromFps;

/// <summary>
/// Phase 4 orchestration tests for step sequencing, conditional execution and audit status.
/// </summary>
public sealed class ScheduledLoadOrchestrationTests
{
    [Fact]
    public async Task ExecuteAsync_ExecutesAllConfiguredSteps_InOrder()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlation = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();

        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(context, steps));

        correlation.GetCorrelationId().Returns("corr-seq");
        var runId = Guid.NewGuid();
        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-seq", Arg.Any<CancellationToken>()).Returns(runId);

        var stepHandlers = CreateHandlersForSteps(context, steps);
        ConfigureStepLifecycle(repository, runId, steps);
        validation.ExecuteAsync(runId, context, steps.Length, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ScheduledLoadValidationAssertionResult>());

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlation,
            validation,
            stepHandlers,
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await sut.ExecuteAsync(CancellationToken.None);

        foreach (var handler in stepHandlers)
        {
            await handler.Received(1).ExecuteAsync(context, Arg.Any<CancellationToken>());
        }

        Received.InOrder(() =>
        {
            repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessPreviousYearTotals, 1, Arg.Any<CancellationToken>());
            repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessCurrentYearTotals, 2, Arg.Any<CancellationToken>());
            repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.DeleteYearsFpsData, 3, Arg.Any<CancellationToken>());
            repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.AddYearsFpsData, 4, Arg.Any<CancellationToken>());
            repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll, 5, Arg.Any<CancellationToken>());
            validation.ExecuteAsync(runId, context, steps.Length, Arg.Any<CancellationToken>());
        });

        await repository.Received(1).CompleteRunAsync(runId, "Completed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SkipsCurrentYearStep_WhenPlanExcludesIt()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlation = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();

        var steps = new[]
        {
            ScheduledLoadFromFpsStep.ProcessPreviousYearTotals,
            ScheduledLoadFromFpsStep.DeleteYearsFpsData,
            ScheduledLoadFromFpsStep.AddYearsFpsData,
            ScheduledLoadFromFpsStep.HandleCurrentYearProjectAll
        };

        var context = new ScheduledLoadFromFpsExecutionContext(3, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(context, steps));

        correlation.GetCorrelationId().Returns("corr-cutover");
        var runId = Guid.NewGuid();
        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-cutover", Arg.Any<CancellationToken>()).Returns(runId);

        var stepHandlers = CreateHandlersForSteps(context, steps);
        ConfigureStepLifecycle(repository, runId, steps);
        validation.ExecuteAsync(runId, context, steps.Length, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ScheduledLoadValidationAssertionResult>());

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlation,
            validation,
            stepHandlers,
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await sut.ExecuteAsync(CancellationToken.None);

        foreach (var handler in stepHandlers)
        {
            await handler.Received(1).ExecuteAsync(context, Arg.Any<CancellationToken>());
        }

        await repository.DidNotReceive().StartStepAsync(
            runId,
            ScheduledLoadFromFpsStep.ProcessCurrentYearTotals,
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenStepThrows_MarksRunAsFailed()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlation = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();

        var steps = new[] { ScheduledLoadFromFpsStep.ProcessPreviousYearTotals };
        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(context, steps));

        correlation.GetCorrelationId().Returns("corr-fail");
        var runId = Guid.NewGuid();
        var stepRunId = Guid.NewGuid();

        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-fail", Arg.Any<CancellationToken>()).Returns(runId);
        repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessPreviousYearTotals, 1, Arg.Any<CancellationToken>()).Returns(stepRunId);

        var failingHandler = Substitute.For<IScheduledLoadFromFpsStepHandler>();
        failingHandler.Step.Returns(ScheduledLoadFromFpsStep.ProcessPreviousYearTotals);
        failingHandler.ExecuteAsync(context, Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("forced failure"));

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlation,
            validation,
            new[] { failingHandler },
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(CancellationToken.None));

        await repository.Received(1).CompleteStepAsync(stepRunId, "Failed", null, "forced failure", Arg.Any<CancellationToken>());
        await repository.Received(1).CompleteRunAsync(runId, "Failed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidationFails_MarksRunAsFailed()
    {
        var planBuilder = Substitute.For<IScheduledLoadFromFpsPlanBuilder>();
        var repository = Substitute.For<IScheduledLoadFromFpsRepository>();
        var correlation = Substitute.For<ICorrelationService>();
        var validation = Substitute.For<ICrossValidationEngine>();

        var steps = new[] { ScheduledLoadFromFpsStep.ProcessPreviousYearTotals };
        var context = new ScheduledLoadFromFpsExecutionContext(7, 2026, 2025, 4);
        planBuilder.Build().Returns(new ScheduledLoadFromFpsExecutionPlan(context, steps));

        correlation.GetCorrelationId().Returns("corr-validation-fail");
        var runId = Guid.NewGuid();
        var stepRunId = Guid.NewGuid();

        repository.StartRunAsync("ScheduledLoadFromFps", 2026, "corr-validation-fail", Arg.Any<CancellationToken>()).Returns(runId);
        repository.StartStepAsync(runId, ScheduledLoadFromFpsStep.ProcessPreviousYearTotals, 1, Arg.Any<CancellationToken>()).Returns(stepRunId);

        var handler = Substitute.For<IScheduledLoadFromFpsStepHandler>();
        handler.Step.Returns(ScheduledLoadFromFpsStep.ProcessPreviousYearTotals);
        handler.ExecuteAsync(context, Arg.Any<CancellationToken>()).Returns(1);

        validation.ExecuteAsync(runId, context, steps.Length, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new ScheduledLoadValidationAssertionResult("ASSERT_001", "failed", 1m, 0m, false, "Expected 1 but got 0")
            });

        var sut = new ScheduledLoadFromFpsJobHandler(
            NullLogger<ScheduledLoadFromFpsJobHandler>.Instance,
            planBuilder,
            repository,
            correlation,
            validation,
            new[] { handler },
            Options.Create(new ScheduledLoadFromFpsSettings { StepTimeoutSeconds = 30 }));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(CancellationToken.None));
        await repository.Received(1).CompleteRunAsync(runId, "Failed", Arg.Any<CancellationToken>());
    }

    private static IReadOnlyList<IScheduledLoadFromFpsStepHandler> CreateHandlersForSteps(
        ScheduledLoadFromFpsExecutionContext context,
        IEnumerable<ScheduledLoadFromFpsStep> steps)
    {
        var handlers = new List<IScheduledLoadFromFpsStepHandler>();
        foreach (var step in steps)
        {
            var handler = Substitute.For<IScheduledLoadFromFpsStepHandler>();
            handler.Step.Returns(step);
            handler.ExecuteAsync(context, Arg.Any<CancellationToken>()).Returns(1);
            handlers.Add(handler);
        }

        return handlers;
    }

    private static void ConfigureStepLifecycle(
        IScheduledLoadFromFpsRepository repository,
        Guid runId,
        IReadOnlyList<ScheduledLoadFromFpsStep> steps)
    {
        for (var i = 0; i < steps.Count; i++)
        {
            repository.StartStepAsync(runId, steps[i], i + 1, Arg.Any<CancellationToken>())
                .Returns(Guid.NewGuid());
        }
    }
}
