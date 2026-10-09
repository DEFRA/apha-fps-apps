using Apha.BatchJobs.Application.Orchestration;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

public sealed class DispatchTimeoutSweepServiceTests
{
    private readonly IJobExecutionRepository _repository = Substitute.For<IJobExecutionRepository>();
    private readonly CapturingLogger<DispatchTimeoutSweepService> _logger = new();

    public DispatchTimeoutSweepServiceTests()
    {
        _repository.FailStaleDispatchedExecutionsAsync(
                Arg.Any<string>(), Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<StaleDispatchedExecution>());
    }

    [Fact]
    public async Task SweepAsync_SweepsEachDispatchedJobWithItsPickupStatusAndClock()
    {
        await new DispatchTimeoutSweepService(_repository, _logger).SweepAsync();

        foreach (var job in new[]
                 {
                     BatchJobNames.BulkTestRatesUpdate, BatchJobNames.BulkStaffRatesUpdate, BatchJobNames.BulkAnimalRatesUpdate,
                     BatchJobNames.YearEndDataSetup, BatchJobNames.YearEndCutover
                 })
        {
            await _repository.Received(1).FailStaleDispatchedExecutionsAsync(
                job, JobStatus.Approved, DispatchClock.ApprovedAt, DispatchTimeoutSweepService.MinimumTimeToLiveMinutes,
                DispatchTimeoutSweepService.TimedOutErrorMessage, DispatchTimeoutSweepService.TimedOutDiagnosticSummary,
                Arg.Any<CancellationToken>());
        }

        await _repository.Received(1).FailStaleDispatchedExecutionsAsync(
            BatchJobNames.RecreateSummary, JobStatus.Initiated, DispatchClock.RequestedAt, DispatchTimeoutSweepService.MinimumTimeToLiveMinutes,
            DispatchTimeoutSweepService.TimedOutErrorMessage, DispatchTimeoutSweepService.TimedOutDiagnosticSummary,
            Arg.Any<CancellationToken>());

        await _repository.Received(6).FailStaleDispatchedExecutionsAsync(
            Arg.Any<string>(), Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Rules_NeverSweepInitiatedForApprovalJobs_OrScheduledJobs()
    {
        // Initiated is a user-held state for maker-checker jobs; scheduled jobs create their own row.
        Assert.DoesNotContain(DispatchTimeoutSweepService.Rules,
            r => r.PickupStatus == JobStatus.Initiated && r.JobName != BatchJobNames.RecreateSummary);
        Assert.DoesNotContain(DispatchTimeoutSweepService.Rules,
            r => r.JobName is BatchJobNames.MabArchive or BatchJobNames.MilestoneUpdateNotifications);
    }

    [Fact]
    public async Task SweepAsync_LogsEachFailedRowWithTheGeneralMarker()
    {
        var failed = new StaleDispatchedExecution(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(-3), 90);
        _repository.FailStaleDispatchedExecutionsAsync(
                BatchJobNames.BulkStaffRatesUpdate, Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([failed]);

        await new DispatchTimeoutSweepService(_repository, _logger).SweepAsync();

        var entry = Assert.Single(_logger.Entries, e => e.Level == LogLevel.Error);
        Assert.Contains(BatchExceptionMarkers.General, entry.Message);
        Assert.Contains(BatchJobNames.BulkStaffRatesUpdate, entry.Message);
        Assert.Contains(failed.JobQueueId.ToString(), entry.Message);
    }

    [Fact]
    public async Task SweepAsync_WhenOneJobFails_LogsAndStillSweepsTheRest()
    {
        _repository.FailStaleDispatchedExecutionsAsync(
                BatchJobNames.BulkTestRatesUpdate, Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<StaleDispatchedExecution>>(new InvalidOperationException("db down")));

        await new DispatchTimeoutSweepService(_repository, _logger).SweepAsync();

        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Error
            && e.Message.Contains(BatchExceptionMarkers.General) && e.Message.Contains(BatchJobNames.BulkTestRatesUpdate));
        await _repository.Received(1).FailStaleDispatchedExecutionsAsync(
            BatchJobNames.RecreateSummary, Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SweepAsync_PropagatesCancellation()
    {
        _repository.FailStaleDispatchedExecutionsAsync(
                Arg.Any<string>(), Arg.Any<JobStatus>(), Arg.Any<DispatchClock>(), Arg.Any<int>(),
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<StaleDispatchedExecution>>(new OperationCanceledException()));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => new DispatchTimeoutSweepService(_repository, _logger).SweepAsync());
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }
}
