using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Task 7 parity tests for legacy sp_LoadFromFPS branch and sequencing behavior.
/// </summary>
public sealed class MabArchiveLoadOrchestratorParityTests
{
    private readonly IReloadFpsTotalsService _totalsService = Substitute.For<IReloadFpsTotalsService>();
    private readonly IMyFpsYearlyDataService _dataService = Substitute.For<IMyFpsYearlyDataService>();
    private readonly IEmailNotificationService _emailNotificationService = Substitute.For<IEmailNotificationService>();
    private readonly IBatchLockRepository _lockRepository = Substitute.For<IBatchLockRepository>();

    private MabArchiveLoadOrchestrator CreateSubject()
    {
        return new MabArchiveLoadOrchestrator(
            _totalsService,
            _dataService,
            _emailNotificationService,
            _lockRepository,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMonthGreaterThan4_RunsPreviousYearThenCurrentYearFullCycle()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var previousYear = 2025;
        var currentYear = 2026;
        var context = new MabArchiveExecutionContext(
            CurrentYear: currentYear,
            PreviousYear: previousYear,
            CurrentMonth: 5,
            PrimaryYear: currentYear,
            IncludePartialRefreshYear: false);

        _dataService.IsYearAvailableAsync(previousYear, ct).Returns(true);
        _dataService.IsYearAvailableAsync(currentYear, ct).Returns(true);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-gt4", context, transactionWrapper, ct);

        Received.InOrder(() =>
        {
            _ = _dataService.IsYearAvailableAsync(previousYear, ct);
            _ = _totalsService.RebuildSourceTotalsAsync(previousYear, ct);
            _ = _dataService.DeleteYearDataAsync(previousYear, ct);
            _ = _dataService.LoadYearDataAsync(previousYear, ct);

            _ = _dataService.IsYearAvailableAsync(currentYear, ct);
            _ = _totalsService.RebuildSourceTotalsAsync(currentYear, ct);
            _ = _dataService.DeleteYearDataAsync(currentYear, ct);
            _ = _dataService.LoadYearDataAsync(currentYear, ct);
        });

        await _dataService.DidNotReceive().RefreshProjectAllOnlyAsync(Arg.Any<int>(), ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMonthLessOrEqual4_RunsPreviousYearFullCycleThenCurrentYearProjectAllOnly()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var previousYear = 2025;
        var currentYear = 2026;
        var context = new MabArchiveExecutionContext(
            CurrentYear: currentYear,
            PreviousYear: previousYear,
            CurrentMonth: 4,
            PrimaryYear: previousYear,
            IncludePartialRefreshYear: true);

        _dataService.IsYearAvailableAsync(previousYear, ct).Returns(true);
        _dataService.IsYearAvailableAsync(currentYear, ct).Returns(true);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-le4", context, transactionWrapper, ct);

        Received.InOrder(() =>
        {
            _ = _dataService.IsYearAvailableAsync(previousYear, ct);
            _ = _totalsService.RebuildSourceTotalsAsync(previousYear, ct);
            _ = _dataService.DeleteYearDataAsync(previousYear, ct);
            _ = _dataService.LoadYearDataAsync(previousYear, ct);

            _ = _dataService.IsYearAvailableAsync(currentYear, ct);
            _ = _dataService.RefreshProjectAllOnlyAsync(currentYear, ct);
        });

        await _totalsService.DidNotReceive().RebuildSourceTotalsAsync(currentYear, ct);
        await _dataService.DidNotReceive().DeleteYearDataAsync(currentYear, ct);
        await _dataService.DidNotReceive().LoadYearDataAsync(currentYear, ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPreviousYearUnavailable_SkipsPreviousYearAndStillProcessesCurrentBranch()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var previousYear = 2025;
        var currentYear = 2026;
        var context = new MabArchiveExecutionContext(
            CurrentYear: currentYear,
            PreviousYear: previousYear,
            CurrentMonth: 5,
            PrimaryYear: currentYear,
            IncludePartialRefreshYear: false);

        _dataService.IsYearAvailableAsync(previousYear, ct).Returns(false);
        _dataService.IsYearAvailableAsync(currentYear, ct).Returns(true);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-prev-missing", context, transactionWrapper, ct);

        await _totalsService.DidNotReceive().RebuildSourceTotalsAsync(previousYear, ct);
        await _dataService.DidNotReceive().DeleteYearDataAsync(previousYear, ct);
        await _dataService.DidNotReceive().LoadYearDataAsync(previousYear, ct);

        await _totalsService.Received(1).RebuildSourceTotalsAsync(currentYear, ct);
        await _dataService.Received(1).DeleteYearDataAsync(currentYear, ct);
        await _dataService.Received(1).LoadYearDataAsync(currentYear, ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCurrentYearUnavailableInPartialRefresh_SkipsProjectAllRefresh()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var previousYear = 2025;
        var currentYear = 2026;
        var context = new MabArchiveExecutionContext(
            CurrentYear: currentYear,
            PreviousYear: previousYear,
            CurrentMonth: 3,
            PrimaryYear: previousYear,
            IncludePartialRefreshYear: true);

        _dataService.IsYearAvailableAsync(previousYear, ct).Returns(true);
        _dataService.IsYearAvailableAsync(currentYear, ct).Returns(false);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-current-missing", context, transactionWrapper, ct);

        await _totalsService.Received(1).RebuildSourceTotalsAsync(previousYear, ct);
        await _dataService.Received(1).DeleteYearDataAsync(previousYear, ct);
        await _dataService.Received(1).LoadYearDataAsync(previousYear, ct);

        await _dataService.DidNotReceive().RefreshProjectAllOnlyAsync(currentYear, ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkFails_SendsFailureNotificationAndRethrows()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var context = new MabArchiveExecutionContext(
            CurrentYear: 2026,
            PreviousYear: 2025,
            CurrentMonth: 6,
            PrimaryYear: 2026,
            IncludePartialRefreshYear: false);

        _dataService.IsYearAvailableAsync(2025, ct).Returns(true);
        _totalsService.RebuildSourceTotalsAsync(2025, ct)
            .Returns(Task.FromException<int>(new InvalidOperationException("boom")));

        Func<Func<Task>, Task> transactionWrapper = work => work();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => subject.ExecuteAsync("run-fail", context, transactionWrapper, ct));

        Assert.Equal("boom", ex.Message);
        await _emailNotificationService.Received(1)
            .SendFailureNotificationAsync(
                "run-fail",
                "MABArchive",
                Arg.Is<string>(m => m.Contains("boom")),
                Arg.Any<DateTime>(),
                ct);
    }
}