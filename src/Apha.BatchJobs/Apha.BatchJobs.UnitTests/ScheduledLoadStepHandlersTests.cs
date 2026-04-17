using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps;
using Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Handlers;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class ScheduledLoadStepHandlersTests
{
    private readonly IScheduledLoadFromFpsRepository _repository = Substitute.For<IScheduledLoadFromFpsRepository>();

    [Fact]
    public async Task DeleteYearsFpsData_WhenBeforeCutover_DeletesPreviousYearOnly()
    {
        var handler = new DeleteYearsFpsDataHandler(_repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteYearsFpsDataHandler>.Instance);
        var context = new ScheduledLoadFromFpsExecutionContext(
            CurrentMonth: 3,
            CurrentYear: 2026,
            PreviousYear: 2025,
            CurrentYearCutoverMonth: 4);

        _repository.DeleteArchiveYearSliceAsync(2025, Arg.Any<CancellationToken>()).Returns(7);

        var rows = await handler.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(7, rows);
        await _repository.Received(1).DeleteArchiveYearSliceAsync(2025, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteArchiveYearSliceAsync(2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddYearsFpsData_WhenAfterCutover_LoadsPreviousAndCurrentYears()
    {
        var handler = new AddYearsFpsDataHandler(_repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<AddYearsFpsDataHandler>.Instance);
        var context = new ScheduledLoadFromFpsExecutionContext(
            CurrentMonth: 7,
            CurrentYear: 2026,
            PreviousYear: 2025,
            CurrentYearCutoverMonth: 4);

        _repository.AddArchiveYearSliceAsync(2025, Arg.Any<CancellationToken>()).Returns(5);
        _repository.AddArchiveYearSliceAsync(2026, Arg.Any<CancellationToken>()).Returns(6);

        var rows = await handler.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(11, rows);
        await _repository.Received(1).AddArchiveYearSliceAsync(2025, Arg.Any<CancellationToken>());
        await _repository.Received(1).AddArchiveYearSliceAsync(2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPreviousYearTotals_UsesPreviousYear()
    {
        var handler = new ProcessPreviousYearTotalsHandler(_repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessPreviousYearTotalsHandler>.Instance);
        var context = new ScheduledLoadFromFpsExecutionContext(6, 2026, 2025, 4);

        _repository.RebuildYearTotalsAsync(2025, Arg.Any<CancellationToken>()).Returns(4);

        var rows = await handler.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(4, rows);
        await _repository.Received(1).RebuildYearTotalsAsync(2025, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessCurrentYearTotals_UsesCurrentYear()
    {
        var handler = new ProcessCurrentYearTotalsHandler(_repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessCurrentYearTotalsHandler>.Instance);
        var context = new ScheduledLoadFromFpsExecutionContext(6, 2026, 2025, 4);

        _repository.RebuildYearTotalsAsync(2026, Arg.Any<CancellationToken>()).Returns(3);

        var rows = await handler.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(3, rows);
        await _repository.Received(1).RebuildYearTotalsAsync(2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleCurrentYearProjectAll_UsesCurrentYear()
    {
        var handler = new HandleCurrentYearProjectAllHandler(_repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<HandleCurrentYearProjectAllHandler>.Instance);
        var context = new ScheduledLoadFromFpsExecutionContext(6, 2026, 2025, 4);

        _repository.RefreshCurrentYearProjectAllAsync(2026, Arg.Any<CancellationToken>()).Returns(8);

        var rows = await handler.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(8, rows);
        await _repository.Received(1).RefreshCurrentYearProjectAllAsync(2026, Arg.Any<CancellationToken>());
    }
}
