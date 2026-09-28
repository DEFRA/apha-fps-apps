using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies matrix-defined column overrides for a given reset phase. Shared by
/// <see cref="Steps.ProjectFinancialResetStep"/> and <see cref="Steps.ConfiguredPlanningResetStep"/>
/// so each table's reset rule lives once in <see cref="YearEndTableRuleMatrix"/>, not per-step.
/// </summary>
internal static class YearEndMatrixResetApplier
{
    public static async Task<int> ApplyResetsForPhaseAsync(
        IYearEndDataSetupRepository repository,
        string resetPhase,
        int targetFpsYear,
        string correlationId,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var entries = YearEndTableRuleMatrix.Entries
            .Where(e => string.Equals(e.ResetPhase, resetPhase, StringComparison.Ordinal))
            .ToList();

        var totalUpdated = 0;

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Overrides is null || entry.Overrides.Count == 0)
            {
                continue;
            }

            if (!await repository.TableExistsAsync(entry.Schema, entry.TableName, cancellationToken))
            {
                logger.LogWarning(
                    "YearEnd reset skipped missing table | CorrelationId={CorrelationId} | ResetPhase={ResetPhase} | Table={Schema}.{Table}",
                    correlationId,
                    resetPhase,
                    entry.Schema,
                    entry.TableName);
                continue;
            }

            var updated = await repository.ResetFieldsByYearAsync(entry.Schema, entry.TableName, "fpsyear", entry.Overrides, targetFpsYear, cancellationToken);
            totalUpdated += updated;

            logger.LogInformation(
                "YearEnd reset completed | CorrelationId={CorrelationId} | ResetPhase={ResetPhase} | Table={Schema}.{Table} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
                correlationId,
                resetPhase,
                entry.Schema,
                entry.TableName,
                targetFpsYear,
                updated);
        }

        return totalUpdated;
    }
}
