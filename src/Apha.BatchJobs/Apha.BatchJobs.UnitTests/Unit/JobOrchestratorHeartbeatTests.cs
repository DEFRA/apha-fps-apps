using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Phase 4 — tests for <see cref="JobOrchestrator"/>'s heartbeat wiring: the
/// <c>Task.WhenAny(jobTask, heartbeatTask)</c> race, renewal semantics, and the
/// <c>IHeartbeatRepositoryScope</c> DbContext-isolation boundary. Complements
/// <c>JobOrchestratorTests</c> (which never exercises heartbeat ticks at all, since every job
/// there completes before the default 30s HeartbeatIntervalSeconds' first tick) and
/// <c>BatchFailureClassifierTests</c>/the <c>IsRetryable</c> tests in <c>JobOrchestratorTests</c>
/// (which cover <see cref="BatchLockLeaseLostException"/>'s classification/retryability directly).
/// See batchjobs-phase4-heartbeat-concurrency-design-2026-09-18.md for the confirmed design.
/// </summary>
public sealed class JobOrchestratorHeartbeatTests
{
    private const string JobName = "HeartbeatTestJob";

    [Fact]
    public async Task RunAsync_JobFinishesFirst_HeartbeatStopsCleanlyAndCompletionIsUnaffected()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        // Long enough to span at least one 1s heartbeat tick before completing, so this actually
        // proves the heartbeat was running and got stopped cleanly — not merely that it never had
        // a chance to start.
        var job = new ControllableJob(JobName, TimeSpan.FromMilliseconds(1500));
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        var result = await orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        Assert.Equal(JobStatus.Completed, result.Status);
        await heartbeatLockRepo.Received().TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_RenewalReturnsFalse_JobCancelledAndLeaseLossSurfaced()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();

        var job = new ControllableJob(JobName, Timeout.InfiniteTimeSpan); // cancelled by the lease loss
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        var ex = await Assert.ThrowsAsync<BatchLockLeaseLostException>(
            () => orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user"));

        Assert.Contains("returned false", ex.Message);
    }

    [Fact]
    public async Task RunAsync_RenewalThrowsOnceThenSucceedsBeforeExpiry_JobContinuesNormally()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        var renewAttempts = 0;
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                renewAttempts++;
                if (renewAttempts == 1)
                    throw new NpgsqlException("transient DB blip");
                return true;
            });
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        // Spans 2+ ticks (1s interval) so the throw-then-succeed sequence both fire, well within
        // the generous 30s LockTimeoutSeconds boundary.
        var job = new ControllableJob(JobName, TimeSpan.FromMilliseconds(2500));
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        var result = await orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.True(renewAttempts >= 2, $"Expected at least 2 renewal attempts (one throw, one success), got {renewAttempts}.");
    }

    [Fact]
    public async Task RunAsync_RenewalKeepsThrowingPastLeaseBoundary_EscalatesToLeaseLoss()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        var renewAttempts = 0;
        // A local function, not a lambda, so its declared `bool` return type disambiguates
        // NSubstitute's Returns<T>(T, Func<CallInfo,T>) overload — a throw-only lambda's return
        // type can't be inferred, which is ambiguous against Task<bool>-specific overloads.
        bool ThrowingRenew(CallInfo _)
        {
            renewAttempts++;
            throw new NpgsqlException("DB unreachable");
        }
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ThrowingRenew);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();

        var job = new ControllableJob(JobName, Timeout.InfiniteTimeSpan); // cancelled once escalation fires
        // 1s heartbeat interval, 1s lease — the very first throw already sits right at the
        // boundary, so escalation happens within a couple of ticks rather than needing a long test.
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 1 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        var ex = await Assert.ThrowsAsync<BatchLockLeaseLostException>(
            () => orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user"));

        Assert.Contains("lease boundary", ex.Message);
        Assert.True(renewAttempts >= 1, "Expected at least one renewal attempt before escalation.");
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task RunAsync_RuntimeTimeoutWithHealthyHeartbeat_StillSurfacesTimeoutException()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var job = new ControllableJob(JobName, Timeout.InfiniteTimeSpan); // cancelled by the runtime timeout
        // HeartbeatIntervalSeconds intentionally longer than JobTimeout, so the heartbeat's first
        // tick never fires before the timeout does — proving case 3 is never confused with case 2
        // even when the heartbeat is otherwise configured healthy.
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 5, LockTimeoutSeconds = 30, JobTimeout = 1 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        await Assert.ThrowsAsync<TimeoutException>(
            () => orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user"));
    }

    [Fact]
    public async Task RunAsync_HostCancellation_PreservesExistingCancellationBehaviour()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var job = new ControllableJob(JobName, Timeout.InfiniteTimeSpan); // cancelled by the host token
        // No runtime timeout configured, and the heartbeat interval is longer than the host
        // cancellation delay below — isolates this to purely the external-cancellation path.
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 5, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings);

        using var hostCts = new CancellationTokenSource();
        hostCts.CancelAfter(TimeSpan.FromMilliseconds(300));

        // ThrowsAnyAsync, not ThrowsAsync: Task.Delay's cancellation throws the subclass
        // TaskCanceledException, not the base OperationCanceledException exactly — matching
        // pre-existing behavior, not a Phase 4 change. The point being proven is that this is
        // some kind of OperationCanceledException, not a TimeoutException or BatchLockLeaseLostException.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user", cancellationToken: hostCts.Token));
    }

    [Fact]
    public async Task RunAsync_UnexpectedHeartbeatException_JobCancelledAndOriginalExceptionSurfaced_NotReclassifiedAsLeaseLoss()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();

        // Simulates the heartbeat's own infrastructure failing (e.g. IDbContextFactory.CreateDbContext
        // throwing) rather than a classified renewal failure — the factory itself throws.
        var heartbeatFactory = Substitute.For<IHeartbeatRepositoryScopeFactory>();
        heartbeatFactory.Create().Returns(_ => throw new InvalidOperationException("heartbeat DbContext factory is broken"));

        var job = new ControllableJob(JobName, Timeout.InfiniteTimeSpan); // cancelled once the heartbeat faults
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, heartbeatFactory, settings);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user"));

        Assert.Equal("heartbeat DbContext factory is broken", ex.Message);
        Assert.IsNotType<BatchLockLeaseLostException>(ex);
    }

    [Fact]
    public async Task RunAsync_TouchReturnsFalse_WarningOnlyNoEscalation()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var job = new ControllableJob(JobName, TimeSpan.FromMilliseconds(1500));
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var capturingLogger = new CapturingLogger<JobOrchestrator>();
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings, capturingLogger);

        var result = await orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Contains(capturingLogger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("updated_at", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RunAsync_TouchThrows_WarningOnlyNoEscalation()
    {
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        // Local function (explicit bool return type), not a lambda — see the comment in
        // RunAsync_RenewalKeepsThrowingPastLeaseBoundary_EscalatesToLeaseLoss for why a
        // throw-only lambda here is ambiguous against Task<bool>-specific Returns overloads.
        static bool ThrowingTouch(CallInfo _) => throw new NpgsqlException("touch write failed");
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ThrowingTouch);

        var job = new ControllableJob(JobName, TimeSpan.FromMilliseconds(1500));
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var capturingLogger = new CapturingLogger<JobOrchestrator>();
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo), settings, capturingLogger);

        var result = await orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Contains(capturingLogger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("TouchRunningExecutionAsync", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RunAsync_Heartbeat_UsesTheScopeFactoryRatherThanTheOrchestratorsOwnRepositories()
    {
        // Architectural proof, not a race: the heartbeat's renew/touch calls land on the repos
        // exposed by IHeartbeatRepositoryScope, never on mainLockRepo/mainExecRepo (the same
        // instances the orchestrator's own acquire/release/execution-record flow uses) — proving
        // it never shares this orchestrator's ambient DbContext with the job. See design note §3.
        var mainLockRepo = BuildHealthyMainLockRepo();
        var mainExecRepo = BuildHealthyMainExecRepo();
        var heartbeatLockRepo = Substitute.For<IBatchLockRepository>();
        heartbeatLockRepo.TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatExecRepo = Substitute.For<IJobExecutionRepository>();
        heartbeatExecRepo.TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var heartbeatFactory = BuildHeartbeatFactory(heartbeatLockRepo, heartbeatExecRepo);

        var job = new ControllableJob(JobName, TimeSpan.FromMilliseconds(1500));
        var settings = Options.Create(new BatchJobSettings { HeartbeatIntervalSeconds = 1, LockTimeoutSeconds = 30 });
        var orchestrator = BuildOrchestrator(job, mainLockRepo, mainExecRepo, heartbeatFactory, settings);

        await orchestrator.RunAsync(JobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        heartbeatFactory.Received(1).Create(); // synchronous member — no await
        await heartbeatLockRepo.Received().TryRenewLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await mainLockRepo.DidNotReceive().TryRenewLockAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await mainExecRepo.DidNotReceive().TouchRunningExecutionAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // ---------- Helpers ----------

    private static JobOrchestrator BuildOrchestrator(
        IBatchJob job,
        IBatchLockRepository mainLockRepo,
        IJobExecutionRepository mainExecRepo,
        IHeartbeatRepositoryScopeFactory heartbeatRepositoryScopeFactory,
        IOptions<BatchJobSettings> settings,
        ILogger<JobOrchestrator>? logger = null)
    {
        var factory = Substitute.For<IBatchJobFactory>();
        factory.Create(job.Name).Returns(job);

        var reconciliationService = Substitute.For<IBatchLockReconciliationService>();
        var correlationService = Substitute.For<ICorrelationContextAccessor>();
        var currentExecutionContext = Substitute.For<ICurrentJobExecutionContext>();
        var notificationService = Substitute.For<IEmailNotificationService>();
        var alertingSettings = Options.Create(new BatchAlertingSettings { EnableEmailNotifications = false });
        var failureClassifier = new BatchFailureClassifier(new ConfigurationBuilder().Build());

        return new JobOrchestrator(
            factory,
            mainLockRepo,
            mainExecRepo,
            reconciliationService,
            heartbeatRepositoryScopeFactory,
            correlationService,
            currentExecutionContext,
            notificationService,
            [],
            alertingSettings,
            settings,
            failureClassifier,
            logger ?? NullLogger<JobOrchestrator>.Instance);
    }

    private static IHeartbeatRepositoryScopeFactory BuildHeartbeatFactory(
        IBatchLockRepository lockRepository, IJobExecutionRepository executionRepository)
    {
        var scope = Substitute.For<IHeartbeatRepositoryScope>();
        scope.LockRepository.Returns(lockRepository);
        scope.ExecutionRepository.Returns(executionRepository);
        scope.DisposeAsync().Returns(ValueTask.CompletedTask);

        var factory = Substitute.For<IHeartbeatRepositoryScopeFactory>();
        factory.Create().Returns(scope);
        return factory;
    }

    private static IBatchLockRepository BuildHealthyMainLockRepo()
    {
        var lockRepo = Substitute.For<IBatchLockRepository>();
        lockRepo.TryAcquireLockAsync(JobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
        // GetLockAsync left unconfigured (returns null) — Layer 2 reconciliation is a no-op, not
        // what these tests are about.
        return lockRepo;
    }

    private static IJobExecutionRepository BuildHealthyMainExecRepo()
    {
        var execRepo = Substitute.For<IJobExecutionRepository>();
        execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult<JobExecutionRecord?>(new JobExecutionRecord
            {
                ExecutionId = 1,
                JobExecutionId = (Guid)args[0],
                JobQueueId = Guid.NewGuid(),
                JobName = JobName,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Initiated,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            }));
        execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(1);
        execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        return execRepo;
    }

    /// <summary>A job that runs for a fixed duration, respecting cancellation exactly like a
    /// cooperative real job would.</summary>
    private sealed class ControllableJob : IBatchJob
    {
        private readonly TimeSpan _runFor;

        public ControllableJob(string name, TimeSpan runFor)
        {
            Name = name;
            _runFor = runFor;
        }

        public string Name { get; }
        public string IdempotencyStrategy => "heartbeat-test";
        public string? ScheduleExpression => null;
        public string? ScheduleDescription => null;
        public int? MaxExecutionSeconds => null;

        public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
            Task.Delay(_runFor, cancellationToken);
    }

    /// <summary>Minimal in-memory logger capturing level + rendered message, so tests can assert what was logged.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (Entries)
            {
                Entries.Add((logLevel, formatter(state, exception)));
            }
        }
    }
}
