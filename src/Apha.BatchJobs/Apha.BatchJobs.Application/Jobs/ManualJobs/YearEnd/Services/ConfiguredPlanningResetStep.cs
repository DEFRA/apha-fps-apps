using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies the <see cref="YearEndResetPhase.ConfiguredPlanningReset"/> column overrides — matrix-driven,
/// currently <c>tblstaffjob</c>/<c>tlkptestreqmt</c>/<c>tblanimalreq</c>/<c>tbladditionalcosts</c> —
/// to target-year rows using strict year scoping. Deliberately does not touch <c>mabarchive.my_*</c>
/// tables: MABArchive participation in Year End is gated exclusively through
/// <see cref="ConditionalMabArchiveYearSetupStep"/>, which defaults to no-op unless separately
/// approved.
/// </summary>
public sealed class ConfiguredPlanningResetStep : IYearEndDataSetupStep
{
    private readonly ILogger<ConfiguredPlanningResetStep> _logger;

    public ConfiguredPlanningResetStep(ILogger<ConfiguredPlanningResetStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConfiguredPlanningResetStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before configured planning reset.");
        }

        var updated = await YearEndMatrixResetHelper.ApplyResetsForPhaseAsync(
            connection,
            transaction,
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

        return context;
    }
}
