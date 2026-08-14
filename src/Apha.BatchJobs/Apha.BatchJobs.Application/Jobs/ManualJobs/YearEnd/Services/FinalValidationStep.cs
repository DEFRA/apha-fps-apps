using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion, inside the same
/// transaction as every prior step. Matrix-driven — dispatches validation per
/// <see cref="YearEndTableRuleMatrix"/> entry by <see cref="YearEndTableRuleMatrixEntry.Role"/>/
/// <see cref="YearEndTableRuleMatrixEntry.Action"/>, not a second hardcoded table list. Because the
/// matrix has no <c>mabarchive</c> entries (MABArchive participation is gated exclusively through
/// <see cref="ConditionalMabArchiveYearSetupStep"/>), this step never expects <c>mabarchive</c>
/// target-year data — there is nothing left to special-case.
/// </summary>
public sealed class FinalValidationStep : IYearEndDataSetupStep
{
    private readonly ILogger<FinalValidationStep> _logger;

    public FinalValidationStep(ILogger<FinalValidationStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "FinalValidationStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before final validation.");
        }

        var currentFpsYear = context.CurrentFpsYear.Value;
        var targetFpsYear = context.TargetFpsYear.Value;

        await ValidateTargetYearMasterStateAsync(connection, transaction, targetFpsYear, cancellationToken);

        foreach (var entry in YearEndTableRuleMatrix.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ValidateEntryAsync(entry, connection, transaction, currentFpsYear, targetFpsYear, context.CorrelationId, cancellationToken);
        }

        _logger.LogInformation(
            "YearEnd final validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            targetFpsYear);

        return context;
    }

    private async Task ValidateEntryAsync(
        YearEndTableRuleMatrixEntry entry,
        DbConnection connection,
        DbTransaction transaction,
        int currentFpsYear,
        int targetFpsYear,
        string correlationId,
        CancellationToken cancellationToken)
    {
        switch (entry.Action)
        {
            case YearEndTableRuleAction.CopyToTargetYear:
                await ValidateCopiedTableAsync(entry, connection, transaction, currentFpsYear, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.AlreadyImplementedViaDedicatedStep:
                // tblperiod today. Exactly-12-target-year-periods enforcement is Phase 3 scope, not
                // yet implemented — this only proves the dedicated step actually produced rows.
                await ValidateDedicatedStepTableHasTargetRowsAsync(entry, connection, transaction, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.ClearTargetYearRows:
                await ValidateTargetYearIsEmptyAsync(entry, connection, transaction, targetFpsYear, cancellationToken);
                break;

            case YearEndTableRuleAction.ValidateExists:
                await ValidateExistsAsync(entry, connection, transaction, targetFpsYear, cancellationToken);
                break;

            default:
                // PendingClassification/CreateTargetYearRow/ResetTargetYearRows/SkipLegacyObsolete/
                // ManualReviewRequired: none of these are ever expected on a live matrix entry by
                // this point (Phase 2 steps 3-4 resolved every entry to one of the actions above).
                // Deliberately no default validation — an unresolved action reaching here is a
                // matrix authoring gap, not something to silently pass or silently skip.
                throw new InvalidOperationException(
                    $"Matrix entry {entry.Schema}.{entry.TableName} has action {entry.Action}, which final validation does not know how to check.");
        }
    }

    private static async Task ValidateTargetYearMasterStateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @target_year;");

        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Target year {targetYear} does not exist in fps.tblyearmaster.");
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        if (!string.Equals(yearStatus, "Planned", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} is in status '{yearStatus}', expected 'Planned' before cutover.");
        }

        if (!active)
        {
            throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
        }
    }

    private static async Task ValidateCopiedTableAsync(
        YearEndTableRuleMatrixEntry entry,
        DbConnection connection,
        DbTransaction transaction,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var sourceCount = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", currentFpsYear, cancellationToken);
        var targetCount = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);

        switch (entry.FinalRowCountRule)
        {
            case YearEndFinalRowCountRule.MatchSource:
                if (targetCount != sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to match source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;

            case YearEndFinalRowCountRule.AtMostSource:
                if (targetCount > sourceCount)
                {
                    throw new InvalidOperationException(
                        $"Table {entry.Schema}.{entry.TableName} expected target-year row count to be at most source-year row count " +
                        $"(source={sourceCount}, target={targetCount}) for year {targetFpsYear}.");
                }

                break;

            case YearEndFinalRowCountRule.NotApplicable:
            default:
                throw new InvalidOperationException(
                    $"Table {entry.Schema}.{entry.TableName} is CopyToTargetYear but has no FinalRowCountRule — matrix authoring gap.");
        }
    }

    private static async Task ValidateDedicatedStepTableHasTargetRowsAsync(
        YearEndTableRuleMatrixEntry entry,
        DbConnection connection,
        DbTransaction transaction,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var count = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);
        if (count <= 0)
        {
            throw new InvalidOperationException(
                $"Expected target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found none.");
        }
    }

    private static async Task ValidateExistsAsync(
        YearEndTableRuleMatrixEntry entry,
        DbConnection connection,
        DbTransaction transaction,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {entry.Schema}.{entry.TableName} does not exist.");
        }

        if (entry.Role == YearEndTableRole.GlobalReference)
        {
            // No fpsyear column, no target-year row concept — structural existence is the whole check.
            return;
        }

        if (!await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", cancellationToken))
        {
            throw new InvalidOperationException($"Required validation table {entry.Schema}.{entry.TableName} does not contain year column fpsyear.");
        }

        var count = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, "fpsyear", targetFpsYear, cancellationToken);
        if (count <= 0)
        {
            throw new InvalidOperationException(
                $"Expected target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear} (year-scoped dependency), but found none.");
        }
    }

    private static async Task ValidateTargetYearIsEmptyAsync(
        YearEndTableRuleMatrixEntry entry,
        DbConnection connection,
        DbTransaction transaction,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
        {
            return;
        }

        var yearColumn = await ResolveYearColumnAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken);
        if (yearColumn is null)
        {
            return;
        }

        var count = await CountByYearAsync(connection, transaction, entry.Schema, entry.TableName, yearColumn, targetFpsYear, cancellationToken);
        if (count != 0)
        {
            throw new InvalidOperationException(
                $"Expected no target-year rows in {entry.Schema}.{entry.TableName} for year {targetFpsYear}, but found {count}.");
        }
    }

    private static async Task<long> CountByYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string yearColumn,
        int year,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @target_year;");
        YearEndSqlHelpers.AddParameter(command, "target_year", year);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, cancellationToken);
    }

    private static async Task<string?> ResolveYearColumnAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "fpsyear", cancellationToken))
        {
            return "fpsyear";
        }

        if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, table, "year", cancellationToken))
        {
            return "year";
        }

        return null;
    }
}
