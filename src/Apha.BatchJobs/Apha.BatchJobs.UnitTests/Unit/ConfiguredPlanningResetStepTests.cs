using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Mocked, no live DB — proves YE-CAP-RESET's behavioral gate in <see cref="ConfiguredPlanningResetStep"/>,
/// and that it does not leak into <see cref="ProjectFinancialResetStep"/> (tlkpproject) or
/// <see cref="CopyFpsYearScopedTablesStep"/> (the copy phase), both of which must stay unaffected by
/// <c>fps.tblsettings.id='CapApprovalReceivedForReset'</c>.
/// </summary>
public sealed class ConfiguredPlanningResetStepTests
{
    private const int TargetFpsYear = 2027;
    private const int CurrentFpsYear = TargetFpsYear - 1;
    private static readonly string[] ConfiguredPlanningTables = ["tbladditionalcosts", "tblanimalreq", "tblstaffjob", "tlkptestreqmt"];

    [Theory]
    [InlineData("Yes")]
    [InlineData("yes")]
    [InlineData("YES")]
    [InlineData("YeS")]
    public async Task ExecuteAsync_WhenCapApprovalIsYesAnyCase_AppliesAllFourConfiguredPlanningResets(string capApprovalValue)
    {
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.GetCapApprovalReceivedForResetSettingAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(capApprovalValue);
        repository.TableExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var step = CreateStep(repository);

        await step.ExecuteAsync(CreateContext());

        foreach (var table in ConfiguredPlanningTables)
        {
            await repository.Received(1).ResetFieldsByYearAsync(
                "fps", table, "fpsyear", Arg.Any<IReadOnlyDictionary<string, string>>(), TargetFpsYear, Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenCapApprovalIsNo_SkipsAllFourResetsWithoutEvenCheckingTablesAndCompletesSuccessfully()
    {
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.GetCapApprovalReceivedForResetSettingAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns("No");

        var step = CreateStep(repository);

        await step.ExecuteAsync(CreateContext());

        await repository.DidNotReceive().TableExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().ResetFieldsByYearAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCapApprovalSettingMissing_ShouldThrowAndNotResetAnything()
    {
        // FPS guarantees this row exists before Data Setup can be initiated or approved — a missing
        // row is an execution failure, never silently treated as No.
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.GetCapApprovalReceivedForResetSettingAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var step = CreateStep(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(CreateContext()));

        Assert.Contains("CapApprovalReceivedForReset", ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().ResetFieldsByYearAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCapApprovalValueIsNeitherYesNorNo_ShouldThrowAndNotResetAnything()
    {
        // No fallback guessing for an unexpected value — fail loudly rather than silently pick a side.
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.GetCapApprovalReceivedForResetSettingAsync(TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns("Maybe");

        var step = CreateStep(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => step.ExecuteAsync(CreateContext()));

        Assert.Contains("unexpected value 'Maybe'", ex.Message, StringComparison.Ordinal);
        await repository.DidNotReceive().ResetFieldsByYearAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProjectFinancialResetStep_TlkpProjectResetStillHappens_RegardlessOfCapApprovalSetting()
    {
        // tlkpproject's reset lives in a separate step and phase (ProjectFinancialReset) that never
        // reads CapApprovalReceivedForReset at all — proves the two are genuinely decoupled, not just
        // coincidentally passing today.
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.TableExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var step = new ProjectFinancialResetStep(repository, NullLogger<ProjectFinancialResetStep>.Instance);

        await step.ExecuteAsync(CreateContext());

        await repository.Received(1).ResetFieldsByYearAsync(
            "fps", "tlkpproject", "fpsyear", Arg.Any<IReadOnlyDictionary<string, string>>(), TargetFpsYear, Arg.Any<CancellationToken>());
        await repository.DidNotReceive().GetCapApprovalReceivedForResetSettingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CopyFpsYearScopedTablesStep_StillCopiesTheFourCapDependentTables_RegardlessOfCapApprovalSetting()
    {
        // The copy phase runs before, and independently of, the reset gate — CAP=No must never prevent
        // the copy itself, only the later post-copy column reset in ConfiguredPlanningResetStep.
        var repository = Substitute.For<IYearEndDataSetupRepository>();
        repository.TableExistsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        repository.ColumnExistsAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", Arg.Any<CancellationToken>())
            .Returns(true);
        repository.CountRowsByYearAsync(Arg.Any<string>(), Arg.Any<string>(), "fpsyear", TargetFpsYear, Arg.Any<CancellationToken>())
            .Returns(0L);

        var step = new CopyFpsYearScopedTablesStep(repository, NullLogger<CopyFpsYearScopedTablesStep>.Instance);

        await step.ExecuteAsync(CreateContext());

        foreach (var table in ConfiguredPlanningTables)
        {
            await repository.Received(1).CopyFpsYearScopedTableAsync(table, CurrentFpsYear, TargetFpsYear, Arg.Any<CancellationToken>());
        }

        await repository.DidNotReceive().GetCapApprovalReceivedForResetSettingAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    private static YearEndExecutionContext CreateContext() =>
        new(Guid.NewGuid().ToString("D"), null, CurrentFpsYear: CurrentFpsYear, TargetFpsYear: TargetFpsYear);

    private static ConfiguredPlanningResetStep CreateStep(IYearEndDataSetupRepository repository) =>
        new(repository, NullLogger<ConfiguredPlanningResetStep>.Instance);
}
