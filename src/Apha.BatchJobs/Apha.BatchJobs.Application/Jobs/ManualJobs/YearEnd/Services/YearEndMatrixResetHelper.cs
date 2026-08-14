using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies matrix-defined column overrides (<see cref="YearEndTableRuleMatrixEntry.Overrides"/>)
/// for whichever <see cref="YearEndTableRuleMatrixEntry.ResetPhase"/> a pipeline reset step owns.
/// Shared by <see cref="ProjectFinancialResetStep"/> and <see cref="ConfiguredPlanningResetStep"/>
/// so the reset rule for each table is represented once in the matrix and executed once, rather
/// than hardcoded independently in every step that used to apply it.
/// </summary>
internal static class YearEndMatrixResetHelper
{
    public static async Task<int> ApplyResetsForPhaseAsync(
        DbConnection connection,
        DbTransaction transaction,
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

            var exists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken);
            if (!exists)
            {
                logger.LogWarning(
                    "YearEnd reset skipped missing table | CorrelationId={CorrelationId} | ResetPhase={ResetPhase} | Table={Schema}.{Table}",
                    correlationId,
                    resetPhase,
                    entry.Schema,
                    entry.TableName);
                continue;
            }

            var setClauses = new List<string>();
            foreach (var (column, value) in entry.Overrides)
            {
                if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, entry.Schema, entry.TableName, column, cancellationToken))
                {
                    setClauses.Add($"{column} = {value}");
                }
            }

            if (setClauses.Count == 0)
            {
                logger.LogWarning(
                    "YearEnd reset found no resettable columns | CorrelationId={CorrelationId} | ResetPhase={ResetPhase} | Table={Schema}.{Table}",
                    correlationId,
                    resetPhase,
                    entry.Schema,
                    entry.TableName);
                continue;
            }

            await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
                UPDATE {entry.Schema}.{entry.TableName}
                SET {string.Join(", ", setClauses)}
                WHERE fpsyear = @target_fpsyear;");

            YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);
            var updated = await command.ExecuteNonQueryAsync(cancellationToken);
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
