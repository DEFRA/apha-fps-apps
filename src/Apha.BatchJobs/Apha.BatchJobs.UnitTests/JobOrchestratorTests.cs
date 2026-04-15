using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly JobOrchestrator _orchestrator;

    public JobOrchestratorTests()
    {
        _orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
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
        capturedCreateStatus.Should().ContainSingle()
            .Which.Should().Be(JobStatus.Running);

        // Assert — execution record updated with Completed status
        capturedUpdateStatus.Should().ContainSingle()
            .Which.Should().Be(JobStatus.Completed);

        // Assert — lock was released
        await _lockRepo.Received(1).ReleaseLockAsync("TestJob", Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Assert — result is correct
        result.Status.Should().Be(JobStatus.Completed);
        result.JobName.Should().Be("TestJob");
        result.RunId.Should().NotBeNullOrEmpty();
        result.ExecutionId.Should().Be(42);
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
        result.Status.Should().Be(JobStatus.Skipped);
        result.Duration.Should().Be(TimeSpan.Zero);
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
        capturedUpdateStatus.Should().ContainSingle()
            .Which.Should().Be(JobStatus.Failed);
        capturedErrorMessage.Should().ContainSingle()
            .Which.Should().Be("Simulated failure");

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
        capturedUpdateStatus.Should().ContainSingle()
            .Which.Should().Be(JobStatus.Cancelled);

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
        result1.RunId.Should().NotBe(result2.RunId);
        Guid.TryParseExact(result1.RunId, "N", out _).Should().BeTrue("RunId should be a valid GUID without dashes");
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

        _lockRepo.TryAcquireLockAsync("ConsistentJob", Arg.Do<string>(id => capturedLockRunId = id),
                 Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);

        _execRepo.CreateExecutionRecordAsync(
                 Arg.Do<JobExecutionRecord>(r => capturedRecordRunId = r.RunId),
                 Arg.Any<CancellationToken>())
                 .Returns(5);

        // Act
        await _orchestrator.RunAsync("ConsistentJob", RunMode.Scheduled);

        // Assert — lock RunId and record RunId are the same
        capturedLockRunId.Should().NotBeNull();
        capturedRecordRunId.Should().NotBeNull();
        capturedLockRunId.Should().Be(capturedRecordRunId);
    }
}
