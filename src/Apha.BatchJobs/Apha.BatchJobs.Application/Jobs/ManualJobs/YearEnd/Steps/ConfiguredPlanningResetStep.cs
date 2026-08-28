using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Applies the <see cref="YearEndResetPhase.ConfiguredPlanningReset"/> column overrides — matrix-driven,
/// currently <c>tblstaffjob</c>/<c>tlkptestreqmt</c>/<c>tblanimalreq</c>/<c>tbladditionalcosts</c> —
/// to target-year rows using strict year scoping. FPS-only: MABArchive participation in Year End is
/// gated exclusively through <see cref="ConditionalMabArchiveYearSetupStep"/>, which defaults to no-op
/// unless separately approved — this step must not reset any <c>mabarchive.my_*</c> table.
/// </summary>
public sealed class ConfiguredPlanningResetStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ConfiguredPlanningResetStep> _logger;

    public ConfiguredPlanningResetStep(
        IYearEndDataSetupRepository repository,
        ILogger<ConfiguredPlanningResetStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConfiguredPlanningResetStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before configured planning reset.");
        }

        var updated = await YearEndMatrixResetApplier.ApplyResetsForPhaseAsync(
            _repository,
            YearEndResetPhase.ConfiguredPlanningReset,
            context.TargetFpsYear.Value,
            context.CorrelationId,
            _logger,
            cancellationToken);

        _logger.LogInformation(
            "YearEnd configured planning reset completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            updated);
    }
}
