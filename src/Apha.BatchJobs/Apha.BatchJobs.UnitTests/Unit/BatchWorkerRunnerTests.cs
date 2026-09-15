using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Worker.Configuration;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchWorkerRunner"/>: request resolution, orchestrator invocation,
/// outcome mapping, and that the summary writer is always invoked exactly once. HealthCheck is
/// intentionally not covered â€” <c>Program.cs</c> never lets it reach this runner.
/// </summary>
public sealed class BatchWorkerRunnerTests
{
    private static IServiceProvider BuildServiceProvider(IJobOrchestrator orchestrator)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => orchestrator);
        return services.BuildServiceProvider();
    }

    private static IHostApplicationLifetime CreateLifetime(out CancellationTokenSource lifetimeCts)
    {
        lifetimeCts = new CancellationTokenSource();
        var hostLifetime = Substitute.For<IHostApplicationLifetime>();
        hostLifetime.ApplicationStopping.Returns(lifetimeCts.Token);
        return hostLifetime;
    }

    private static BatchWorkerRunner CreateRunner(
        IJobOrchestrator orchestrator,
        RecordingSummaryWriter summaryWriter,
        IHostApplicationLifetime? hostLifetime,
        int overallTimeoutSeconds) =>
        CreateRunner(new BatchExecutionRequestResolver(), orchestrator, summaryWriter, hostLifetime, overallTimeoutSeconds);

    private static BatchWorkerRunner CreateRunner(
        BatchExecutionRequestResolver resolver,
        IJobOrchestrator orchestrator,
        RecordingSummaryWriter summaryWriter,
        IHostApplicationLifetime? hostLifetime,
        int overallTimeoutSeconds) =>
        new(
            resolver,
            hostLifetime ?? CreateLifetime(out _),
            Options.Create(new BatchRuntimeOptions { WorkerOverallTimeoutSeconds = overallTimeoutSeconds }),
            BuildServiceProvider(orchestrator),
            new BatchFailureClassifier(new ConfigurationBuilder().Build()),
            summaryWriter,
            NullLogger<BatchWorkerRunner>.Instance);

    private static async Task<JobExecutionResult> WaitForCancellationAsync(CancellationToken token)
    {
        await Task.Delay(Timeout.Infinite, token);
        throw new InvalidOperationException("unreachable â€” Task.Delay should have thrown first");
    }

    [Fact]
    public async Task RunAsync_OnSuccess_ReturnsSuccessExitCodeAndWritesSummaryOnce()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new JobExecutionResult(Guid.NewGuid(), "RecreateSummary", JobStatus.Completed, TimeSpan.FromSeconds(1), 1));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Success, exitCode);
        Assert.Equal(1, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Success, summaryWriter.LastResult!.Outcome);
    }

    [Fact]
    public async Task RunAsync_WhenRequestResolutionFails_NeverCallsOrchestrator()
    {
        using var scope = new EnvScopeSet("<jobName>", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.ConfigurationFailure, exitCode);
        Assert.Equal(1, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Failure, summaryWriter.LastResult!.Outcome);
        await orchestrator.DidNotReceive().RunAsync(
            Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenOrchestratorThrowsJobLockException_MapsToLockFailure()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobLockException("already running")));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.LockFailure, exitCode);
        Assert.Equal(BatchFailureCategory.Concurrency, summaryWriter.LastResult!.FailureCategory);
        Assert.Equal(1, summaryWriter.CallCount);
    }

    [Fact]
    public async Task RunAsync_WhenHostShutdownRequested_MapsToCancelledWithHostShutdownReason()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var hostLifetime = CreateLifetime(out var lifetimeCts);
        lifetimeCts.Cancel();

        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new OperationCanceledException()));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(BatchRunOutcome.Cancelled, summaryWriter.LastResult!.Outcome);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.HostShutdown, summaryWriter.LastResult!.CancellationReason);
    }

    [Fact]
    public async Task RunAsync_WhenOverallTimeoutFires_MapsToCancelledWithTimeoutReason()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WaitForCancellationAsync((CancellationToken)callInfo[6]));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 1);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.Timeout, summaryWriter.LastResult!.CancellationReason);
    }

    [Fact]
    public async Task RunAsync_WhenCancelledWithoutShutdownOrTimeout_MapsToUnclassified()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new OperationCanceledException()));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.Unclassified, summaryWriter.LastResult!.CancellationReason);
    }

    private static EnvScopeSet FanOutEnvScope() =>
        new("MonthlyBusinessNotifications", "Scheduled", null, "EventBridgeScheduler");

    private static JobExecutionResult SucceededResult(string jobName) =>
        new(Guid.NewGuid(), jobName, JobStatus.Completed, TimeSpan.FromSeconds(1), 1);

    [Fact]
    public async Task RunAsync_WhenCategoryFanOutBothChildrenSucceed_ReturnsSuccessAndWritesTwoSummaries()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(SucceededResult((string)callInfo[0])));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(resolver, orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Success, exitCode);
        Assert.Equal(2, summaryWriter.CallCount);
        Assert.All(summaryWriter.AllResults, r => Assert.Equal(BatchRunOutcome.Success, r.Outcome));
        Assert.Equal(["TestJobA", "TestJobB"], summaryWriter.AllResults.Select(r => r.JobName));
    }

    [Fact]
    public async Task RunAsync_WhenFirstChildFailsAndSecondSucceeds_ReturnsFirstFailureExitCodeAndAttemptsBoth()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync("TestJobA", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobLockException("already running")));
        orchestrator.RunAsync("TestJobB", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SucceededResult("TestJobB"));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(resolver, orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.LockFailure, exitCode);
        Assert.Equal(2, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Failure, summaryWriter.AllResults[0].Outcome);
        Assert.Equal(BatchRunOutcome.Success, summaryWriter.AllResults[1].Outcome);
    }

    [Fact]
    public async Task RunAsync_WhenFirstChildSucceedsAndSecondFails_ReturnsSecondFailureExitCode()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync("TestJobA", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(SucceededResult("TestJobA"));
        orchestrator.RunAsync("TestJobB", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobLockException("already running")));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(resolver, orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.LockFailure, exitCode);
        Assert.Equal(2, summaryWriter.CallCount);
    }

    [Fact]
    public async Task RunAsync_WhenBothChildrenFailDifferently_ReturnsFirstFailureExitCode()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync("TestJobA", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobLockException("already running")));
        orchestrator.RunAsync("TestJobB", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobValidationException("bad parameters")));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(resolver, orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.LockFailure, exitCode);
        Assert.Equal(2, summaryWriter.CallCount);
        Assert.Equal(BatchFailureCategory.Concurrency, summaryWriter.AllResults[0].FailureCategory);
        Assert.Equal(BatchFailureCategory.Configuration, summaryWriter.AllResults[1].FailureCategory);
    }

    [Fact]
    public async Task RunAsync_WhenCancellationFiresAfterFirstChildSucceeds_StopsRemainingChildrenAndReportsNonSuccess()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var hostLifetime = CreateLifetime(out var lifetimeCts);
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync("TestJobA", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // Simulates the overall-timeout/host-shutdown token firing right as this child finishes.
                lifetimeCts.Cancel();
                return Task.FromResult(SucceededResult("TestJobA"));
            });
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(resolver, orchestrator, summaryWriter, hostLifetime, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.NotEqual(BatchExitCodes.Success, exitCode);
        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(1, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Success, summaryWriter.LastResult!.Outcome);
        await orchestrator.DidNotReceive().RunAsync("TestJobB", Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCategoryFanOutWithTwoChildren_UsesSeparateDIScopePerChild()
    {
        using var scope = FanOutEnvScope();
        var resolver = new BatchExecutionRequestResolver(["TestJobA", "TestJobB"]);
        var resolvedOrchestrators = new List<IJobOrchestrator>();
        var services = new ServiceCollection();
        services.AddScoped<IJobOrchestrator>(_ =>
        {
            var fake = Substitute.For<IJobOrchestrator>();
            fake.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(SucceededResult((string)callInfo[0])));
            resolvedOrchestrators.Add(fake);
            return fake;
        });

        var runner = new BatchWorkerRunner(
            resolver,
            CreateLifetime(out _),
            Options.Create(new BatchRuntimeOptions { WorkerOverallTimeoutSeconds = 3600 }),
            services.BuildServiceProvider(),
            new BatchFailureClassifier(new ConfigurationBuilder().Build()),
            new RecordingSummaryWriter(),
            NullLogger<BatchWorkerRunner>.Instance);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Success, exitCode);
        Assert.Equal(2, resolvedOrchestrators.Count);
        Assert.NotSame(resolvedOrchestrators[0], resolvedOrchestrators[1]);
    }

    private sealed class RecordingSummaryWriter : IBatchRunSummaryWriter
    {
        public int CallCount { get; private set; }
        public BatchExecutionResult? LastResult { get; private set; }
        public List<BatchExecutionResult> AllResults { get; } = [];

        public void WriteSummary(BatchExecutionResult result, TimeSpan duration)
        {
            CallCount++;
            LastResult = result;
            AllResults.Add(result);
        }
    }

    private sealed class EnvScopeSet : IDisposable
    {
        private readonly List<EnvScope> _scopes = [];

        public EnvScopeSet(string? jobName, string? runMode, string? jobExecutionId, string? requestedBy)
        {
            _scopes.Add(new EnvScope("BATCH_JOB_NAME", jobName));
            _scopes.Add(new EnvScope("BATCH_RUN_MODE", runMode));
            _scopes.Add(new EnvScope("BATCH_JOB_EXECUTION_ID", jobExecutionId));
            _scopes.Add(new EnvScope("BATCH_EXECUTION_ID", null));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_BY", requestedBy));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_AT_UTC", null));
            _scopes.Add(new EnvScope("BATCH_JOB_PARAMETERS_JSON", null));
        }

        public void Dispose()
        {
            foreach (var envScope in _scopes)
                envScope.Dispose();
        }
    }

    private sealed class EnvScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _original;

        public EnvScope(string name, string? value)
        {
            _name = name;
            _original = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(_name, _original);
    }
}
