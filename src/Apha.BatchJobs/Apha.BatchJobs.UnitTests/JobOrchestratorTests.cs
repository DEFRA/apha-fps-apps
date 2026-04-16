using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="JobOrchestrator"/> covering the full execution lifecycle:
/// lock acquire → record start → execute → record complete → release lock.
/// </summary>
public sealed class JobOrchestratorTests
{
    private readonly IBatchJobFactory _factory = Substitute.For<IBatchJobFactory>();
    private readonly IBatchLockRepository _lockRepo = Substitute.For<IBatchLockRepository>();
    private readonly IJobExecutionRepository _execRepo = Substitute.For<IJobExecutionRepository>();
    private readonly IOptions<BatchJobSettings> _settings = Options.Create(new BatchJobSettings { JobTimeout = 3600 });
    private readonly JobOrchestrator _orchestrator;

    public JobOrchestratorTests()
    {
        _orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _settings,
            NullLogger<JobOrchestrator>.Instance);
    }

    // ─────────────────────────────────────────────────────────────
    // Happy path: lock acquired, job succeeds
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenLockAcquired_ExecutesJobAndWritesRecords()
    {
        // Arrange — capture argument state at call time (not at assertion time)
        // because JobExecutionRecord is a mutable reference type that gets updated
        // between CreateExecutionRecordAsync and UpdateExecutionRecordAsync.
        var capturedCreateStatus = new List<JobStatus>();
        var capturedUpdateStatus = new List<JobStatus>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("TestJob");

        _factory.Create("TestJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("TestJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedCreateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedUpdateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.RunAsync("TestJob", RunMode.AdHoc);

        // Assert — job was called
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());

        // Assert — execution record created with Running status
        Assert.Single(capturedCreateStatus);
        Assert.Equal(JobStatus.Running, capturedCreateStatus[0]);

        // Assert — execution record updated with Completed status
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Completed, capturedUpdateStatus[0]);

        // Assert — lock was released
        await _lockRepo.Received(1).ReleaseLockAsync("TestJob", Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Assert — result is correct
        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Equal("TestJob", result.JobName);
        Assert.False(string.IsNullOrWhiteSpace(result.RunId));
        Assert.Equal(42, result.ExecutionId);
    }

    // ─────────────────────────────────────────────────────────────
    // Lock already held — job must be skipped (not executed)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenLockNotAcquired_SkipsExecutionAndReturnsSkipped()
    {
        // Arrange
        _lockRepo.TryAcquireLockAsync("TestJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(false);

        // Act
        var result = await _orchestrator.RunAsync("TestJob", RunMode.Scheduled);

        // Assert — job factory was never called
        _factory.DidNotReceive().Create(Arg.Any<string>());

        // Assert — no execution records written
        await _execRepo.DidNotReceive().CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());

        // Assert — result indicates skip
        Assert.Equal(JobStatus.Skipped, result.Status);
        Assert.Equal(TimeSpan.Zero, result.Duration);
    }

    // ─────────────────────────────────────────────────────────────
    // Job throws — record must be written as Failed, lock released
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenJobFails_WritesFailedRecordAndReleasesLock()
    {
        // Arrange
        var capturedUpdateStatus = new List<JobStatus>();
        var capturedErrorMessage = new List<string?>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("FailingJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
           .Returns(Task.FromException(new InvalidOperationException("Simulated failure")));

        _factory.Create("FailingJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("FailingJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(99);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => { capturedUpdateStatus.Add(r.Status); capturedErrorMessage.Add(r.ErrorMessage); }),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — orchestrator should re-throw
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("FailingJob", RunMode.Scheduled));

        // Assert — failure record written
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Failed, capturedUpdateStatus[0]);
        Assert.Single(capturedErrorMessage);
        Assert.Equal("Simulated failure", capturedErrorMessage[0]);

        // Assert — lock still released even after failure
        await _lockRepo.Received(1).ReleaseLockAsync("FailingJob", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // Cancellation — record written as Cancelled, lock released
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenCancelled_WritesCancelledRecordAndReleasesLock()
    {
        // Arrange
        var capturedUpdateStatus = new List<JobStatus>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("CancellableJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
           .Returns(Task.FromException<Task>(new OperationCanceledException()));

        _factory.Create("CancellableJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("CancellableJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(77);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedUpdateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — should re-throw as OperationCanceledException
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _orchestrator.RunAsync("CancellableJob", RunMode.AdHoc));

        // Assert — cancelled record written
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Cancelled, capturedUpdateStatus[0]);

        // Assert — lock released
        await _lockRepo.Received(1).ReleaseLockAsync("CancellableJob", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // RunId is a valid GUID per run
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_GeneratesUniqueRunIdPerExecution()
    {
        // Arrange
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("IdJob");

        _factory.Create("IdJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("IdJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(1);

        // Act
        var result1 = await _orchestrator.RunAsync("IdJob", RunMode.AdHoc);
        var result2 = await _orchestrator.RunAsync("IdJob", RunMode.AdHoc);

        // Assert — each run gets a different RunId
        Assert.NotEqual(result2.RunId, result1.RunId);
        Assert.True(Guid.TryParseExact(result1.RunId, "N", out _), "RunId should be a valid GUID without dashes");
    }

    // ─────────────────────────────────────────────────────────────
    // Lock is passed the RunId that the execution record also uses
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_UsesConsistentRunIdAcrossLockAndExecutionRecord()
    {
        // Arrange
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("ConsistentJob");

        _factory.Create("ConsistentJob").Returns(job);

        string? capturedLockRunId = null;
        string? capturedRecordRunId = null;
        string? capturedReleaseRunId = null;

        _lockRepo.TryAcquireLockAsync("ConsistentJob", Arg.Do<string>(id => capturedLockRunId = id),
                 Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);

        _execRepo.CreateExecutionRecordAsync(
                 Arg.Do<JobExecutionRecord>(r => capturedRecordRunId = r.RunId),
                 Arg.Any<CancellationToken>())
                 .Returns(5);

        _lockRepo.ReleaseLockAsync("ConsistentJob", Arg.Do<string>(id => capturedReleaseRunId = id), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.RunAsync("ConsistentJob", RunMode.Scheduled);

        // Assert — RunId correlation is preserved across lock acquire, execution record, lock release, and final result.
        Assert.NotNull(capturedLockRunId);
        Assert.NotNull(capturedRecordRunId);
        Assert.NotNull(capturedReleaseRunId);
        Assert.Equal(capturedRecordRunId, capturedLockRunId);
        Assert.Equal(capturedRecordRunId, capturedReleaseRunId);
        Assert.Equal(capturedRecordRunId, result.RunId);
    }

    // ─────────────────────────────────────────────────────────────
    // Concurrent lock contention — only first caller wins
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenTwoConcurrentCallsForSameJob_OnlyOneExecutesAndOtherIsSkipped()
    {
        // Arrange — first call acquires lock, second call gets false (DB unique constraint)
        var lockCallCount = 0;
        _lockRepo.TryAcquireLockAsync("ConcurrentJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(_ => ++lockCallCount == 1);  // first true, subsequent false

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("ConcurrentJob");
        _factory.Create("ConcurrentJob").Returns(job);

        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(1);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — simulate two concurrent runs
        var task1 = _orchestrator.RunAsync("ConcurrentJob", RunMode.Scheduled);
        var task2 = _orchestrator.RunAsync("ConcurrentJob", RunMode.Scheduled);
        var results = await Task.WhenAll(task1, task2);

        // Assert — exactly one completed, one skipped
        Assert.Equal(1, results.Count(r => r.Status == JobStatus.Completed));
        Assert.Equal(1, results.Count(r => r.Status == JobStatus.Skipped));

        // Assert — job executed exactly once
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenFirstAttemptFailsAndSecondSucceeds_RetriesAndCompletes()
    {
        // Arrange
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 1,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            retrySettings,
            NullLogger<JobOrchestrator>.Instance);

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("RetrySuccessJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException(new TimeoutException("Transient failure")),
                Task.CompletedTask);

        _factory.Create("RetrySuccessJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("RetrySuccessJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(200);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await orchestrator.RunAsync("RetrySuccessJob", RunMode.AdHoc);

        // Assert
        Assert.Equal(JobStatus.Completed, result.Status);
        await job.Received(2).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenRetriesExhausted_ThrowsAfterConfiguredAttempts()
    {
        // Arrange
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 2,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            retrySettings,
            NullLogger<JobOrchestrator>.Instance);

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("RetryFailJob");
        // Use explicit transient infrastructure exception so all attempts run.
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("Transient persistent failure")));

        _factory.Create("RetryFailJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("RetryFailJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(201);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync<TimeoutException>(
            () => orchestrator.RunAsync("RetryFailJob", RunMode.Scheduled));

        // Assert (first attempt + 2 retries)
        await job.Received(3).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // Non-retryable exceptions — should fail on first attempt even when retries configured
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(NotSupportedException))]
    public async Task RunAsync_WhenNonRetryableExceptionThrown_FailsImmediatelyWithoutRetry(Type exceptionType)
    {
        // Arrange — 2 retries configured, but non-retryable exception must stop on attempt 1
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 2,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            retrySettings,
            NullLogger<JobOrchestrator>.Instance);

        var nonRetryableEx = (Exception)Activator.CreateInstance(exceptionType, "Non-retryable")!;
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("NonRetryableJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(nonRetryableEx));

        _factory.Create("NonRetryableJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("NonRetryableJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(300);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync(exceptionType,
            () => orchestrator.RunAsync("NonRetryableJob", RunMode.AdHoc));

        // Assert — only one attempt, no retries
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsRetryable_ClassifiesExceptionTypesCorrectly()
    {
        Assert.False(JobOrchestrator.IsRetryable(new OperationCanceledException()));
        Assert.False(JobOrchestrator.IsRetryable(new ArgumentException()));
        Assert.False(JobOrchestrator.IsRetryable(new InvalidOperationException()));
        Assert.False(JobOrchestrator.IsRetryable(new NotSupportedException()));
        Assert.False(JobOrchestrator.IsRetryable(new NotImplementedException()));
        Assert.False(JobOrchestrator.IsRetryable(new Exception("generic transient")));
        Assert.True(JobOrchestrator.IsRetryable(new TimeoutException()));
    }

    [Fact]
    public async Task RunAsync_UsesConfiguredLockTimeoutForLockAcquisition()
    {
        // Arrange
        var timeoutSettings = Options.Create(new BatchJobSettings { LockTimeoutSeconds = 1234 });
        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            timeoutSettings,
            NullLogger<JobOrchestrator>.Instance);

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("TimeoutJob");
        _factory.Create("TimeoutJob").Returns(job);

        _lockRepo.TryAcquireLockAsync("TimeoutJob", Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(11);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await orchestrator.RunAsync("TimeoutJob", RunMode.Scheduled);

        // Assert
        await _lockRepo.Received(1).TryAcquireLockAsync(
            "TimeoutJob",
            Arg.Any<string>(),
            1234,
            Arg.Any<CancellationToken>());
    }
}
