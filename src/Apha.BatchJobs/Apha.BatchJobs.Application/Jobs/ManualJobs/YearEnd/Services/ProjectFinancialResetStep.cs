using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies the <see cref="YearEndResetPhase.ProjectFinancialReset"/> column overrides — matrix-driven,
/// currently just <c>tlkpproject</c>'s financial/planning fields — to target-year rows using strict
/// year scoping. Deliberately does not touch <c>mabarchive.my_*</c> tables: MABArchive participation
/// in Year End is gated exclusively through <see cref="ConditionalMabArchiveYearSetupStep"/>, which
/// defaults to no-op unless separately approved.
/// </summary>
public sealed class ProjectFinancialResetStep : IYearEndDataSetupStep
{
    private readonly ILogger<ProjectFinancialResetStep> _logger;

    public ProjectFinancialResetStep(ILogger<ProjectFinancialResetStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ProjectFinancialResetStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before project financial reset.");
        }

        var updated = await YearEndMatrixResetHelper.ApplyResetsForPhaseAsync(
            connection,
            transaction,
            YearEndResetPhase.ProjectFinancialReset,
            context.TargetFpsYear.Value,
            context.CorrelationId,
            _logger,
            cancellationToken);

        _logger.LogInformation(
            "YearEnd project financial reset completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            updated);

        return context;
    }
}
