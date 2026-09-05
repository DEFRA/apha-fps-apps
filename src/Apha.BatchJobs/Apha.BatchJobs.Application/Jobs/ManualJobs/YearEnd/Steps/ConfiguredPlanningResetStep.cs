using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Resets planning columns on tblstaffjob/tlkptestreqmt/tblanimalreq/tbladditionalcosts for the
/// target year (matrix-driven) — gated on <c>fps.tblsettings.id = 'CapApprovalReceivedForReset'</c>
/// for the target year (YE-CAP-RESET). FPS-only — never touches mabarchive tables.
/// </summary>
/// <remarks>
/// Only this step's reset phase is CAP-gated. <see cref="ProjectFinancialResetStep"/> (tlkpproject) is
/// unconditional and untouched by this gate — its reset is not CAP-dependent. Copying the four affected
/// tables into the target year (<see cref="CopyFpsYearScopedTablesStep"/>) happens earlier in the
/// pipeline regardless of this gate; only the post-copy column reset is conditional.
/// </remarks>
public sealed class ConfiguredPlanningResetStep : IYearEndDataSetupStep
{
    private const string CapApprovalSettingId = "CapApprovalReceivedForReset";

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

        var targetFpsYear = context.TargetFpsYear.Value;

        var capApproval = await _repository.GetCapApprovalReceivedForResetSettingAsync(targetFpsYear, cancellationToken);

        if (capApproval is null)
        {
            throw new InvalidOperationException(
                $"Required setting fps.tblsettings.id='{CapApprovalSettingId}' was not found for target year {targetFpsYear}. " +
                "FPS is expected to guarantee this setting exists before Year End Data Setup can be initiated or approved.");
        }

        if (string.Equals(capApproval, "No", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "YearEnd configured planning reset skipped | CorrelationId={CorrelationId} | TargetYear={TargetYear} | Reason={SettingId}=No",
                context.CorrelationId,
                targetFpsYear,
                CapApprovalSettingId);
            return;
        }

        if (!string.Equals(capApproval, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Setting fps.tblsettings.id='{CapApprovalSettingId}' for target year {targetFpsYear} has unexpected value '{capApproval}' — expected 'Yes' or 'No'.");
        }

        var updated = await YearEndMatrixResetApplier.ApplyResetsForPhaseAsync(
            _repository,
            YearEndResetPhase.ConfiguredPlanningReset,
            targetFpsYear,
            context.CorrelationId,
            _logger,
            cancellationToken);

        _logger.LogInformation(
            "YearEnd configured planning reset completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
            context.CorrelationId,
            targetFpsYear,
            updated);
    }
}
