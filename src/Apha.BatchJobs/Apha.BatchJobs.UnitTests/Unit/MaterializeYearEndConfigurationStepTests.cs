using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Mocked, no live DB — proves <see cref="MaterializeYearEndConfigurationStep"/>'s branching logic in
/// isolation. Materializes using <c>context.TargetFpsYear</c>; <c>job_queue.target_fpsyear</c> is not
/// read for this purpose (the FPS API does not currently persist it at Initiate time).
/// Column-mapping correctness (staging → real table) is proven separately, against a live DB, by
/// <c>YearEndDataSetupRepositoryMaterializationIntegrationTests</c>.
/// </summary>
public sealed class MaterializeYearEndConfigurationStepTests
{
    private const int TargetFpsYear = 2026;
    private const string SettingsTable = "tblsettings";
    private const string MonthHoursTable = "tlkpmonthhours";

    [Fact]
    public async Task ExecuteAsync_WhenTargetFpsYearMissing_ShouldThrow()
    {
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        var step = CreateStep(repository);
        var context = new YearEndExecutionContext(Guid.NewGuid().ToString("D"), null, CurrentFpsYear: 2025, TargetFpsYear: null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains("targetFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive().ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCorrelationIdIsNotAGuid_ShouldThrow()
    {
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        var step = CreateStep(repository);
        var context = new YearEndExecutionContext("not-a-guid", null, CurrentFpsYear: 2025, TargetFpsYear: TargetFpsYear);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains("not a valid JobExecutionId", ex.Message, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive().ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoJobQueueRowResolved_ShouldThrow()
    {
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)null);

        var step = CreateStep(repository);
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains("No fps.job_queue row found", ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().MaterializeStagedSettingsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().MaterializeStagedMonthHoursAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetYearAlreadyHasSettingsRows_ShouldThrowAndMaterializeNothing()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, TargetFpsYear));
        repository.CountRowsByYearAsync("fps", SettingsTable, "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(3L);

        var step = CreateStep(repository);
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains(SettingsTable, ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().MaterializeStagedSettingsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().MaterializeStagedMonthHoursAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetYearAlreadyHasMonthHoursRows_ShouldThrowAndMaterializeNothing()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, TargetFpsYear));
        repository.CountRowsByYearAsync("fps", SettingsTable, "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);
        repository.CountRowsByYearAsync("fps", MonthHoursTable, "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(15L);

        var step = CreateStep(repository);
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains(MonthHoursTable, ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().MaterializeStagedSettingsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().MaterializeStagedMonthHoursAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoStagedSettingsMaterialized_ShouldThrow()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, TargetFpsYear));
        repository.CountRowsByYearAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);
        repository.MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0);

        var step = CreateStep(repository);
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains("No staged settings found", ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().MaterializeStagedMonthHoursAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoStagedMonthHoursMaterialized_ShouldThrow()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, TargetFpsYear));
        repository.CountRowsByYearAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);
        repository.MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(1);
        repository.MaterializeStagedMonthHoursAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0);

        var step = CreateStep(repository);
        var context = CreateContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(context));

        Assert.Contains("No staged month hours found", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_MaterializesUsingTheContextTargetFpsYear()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, TargetFpsYear));
        repository.CountRowsByYearAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);
        repository.MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(1);
        repository.MaterializeStagedMonthHoursAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(15);

        var step = CreateStep(repository);
        var context = CreateContext();

        await step.ExecuteAsync(context);

        await repository.Received(1).MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>());
        await repository.Received(1).MaterializeStagedMonthHoursAsync(TargetFpsYear, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersistedTargetFpsYearIsNullOrDiffers_StillMaterializesUsingContext()
    {
        var jobQueueId = Guid.NewGuid();
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.ResolveJobQueueByExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(((Guid JobQueueId, int? TargetFpsYear)?)(jobQueueId, null));
        repository.CountRowsByYearAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);
        repository.MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(1);
        repository.MaterializeStagedMonthHoursAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(15);

        var step = CreateStep(repository);
        var context = CreateContext();

        await step.ExecuteAsync(context);

        await repository.Received(1).MaterializeStagedSettingsAsync(TargetFpsYear, Arg.Any<CancellationToken>());
        await repository.Received(1).MaterializeStagedMonthHoursAsync(TargetFpsYear, Arg.Any<CancellationToken>());
    }

    private static YearEndExecutionContext CreateContext() =>
        new(Guid.NewGuid().ToString("D"), null, CurrentFpsYear: 2025, TargetFpsYear: TargetFpsYear);

    private static MaterializeYearEndConfigurationStep CreateStep(IYearEndDataSetupRepository repository) =>
        new(repository, NullLogger<MaterializeYearEndConfigurationStep>.Instance);
}
