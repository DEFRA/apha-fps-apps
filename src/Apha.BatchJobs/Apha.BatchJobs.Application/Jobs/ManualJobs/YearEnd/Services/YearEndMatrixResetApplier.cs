using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies matrix-defined column overrides (<see cref="YearEndTableRuleMatrixEntry.Overrides"/>) for
/// whichever <see cref="YearEndTableRuleMatrixEntry.ResetPhase"/> a pipeline reset step owns. Shared
/// by <see cref="Steps.ProjectFinancialResetStep"/> and <see cref="Steps.ConfiguredPlanningResetStep"/>
/// so the reset rule for each table is represented once in the matrix and executed once, rather than
/// hardcoded independently in every step that applies it. Table-name source is
/// <see cref="YearEndTableRuleMatrix"/> only — never <c>mabarchive</c>, since the matrix has no
/// <c>mabarchive</c> entries.
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
