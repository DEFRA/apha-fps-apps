using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Clears target-year rows from every <see cref="YearEndTableRuleMatrix"/> entry classified
/// <see cref="YearEndTableRuleAction.ClearTargetYearRows"/> (the spec §19 "must start empty in the
/// target year" legacy candidates). Matrix-driven — no local table list; the matrix is the single
/// authoritative list, also consumed by <see cref="FinalValidationStep"/> to verify the result.
/// </summary>
public sealed class TargetYearEmptyTablesStep : IYearEndDataSetupStep
{
    private readonly ILogger<TargetYearEmptyTablesStep> _logger;

    public TargetYearEmptyTablesStep(ILogger<TargetYearEmptyTablesStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "TargetYearEmptyTablesStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before target-year empty-table cleanup.");
        }

        var clearEntries = YearEndTableRuleMatrix.Entries
            .Where(e => e.Action == YearEndTableRuleAction.ClearTargetYearRows)
            .ToList();

        var totalDeleted = 0;

        foreach (var entry in clearEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await YearEndSqlHelpers.TableExistsAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken))
            {
                continue;
            }

            var yearColumn = await ResolveYearColumnAsync(connection, transaction, entry.Schema, entry.TableName, cancellationToken);
            if (yearColumn is null)
            {
                _logger.LogWarning(
                    "YearEnd empty-table cleanup skipped non-year-scoped table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    entry.Schema,
                    entry.TableName);
                continue;
            }

            var deleted = await DeleteByYearAsync(
                connection,
                transaction,
                entry.Schema,
                entry.TableName,
                yearColumn,
                context.TargetFpsYear.Value,
                cancellationToken);

            totalDeleted += deleted;

            _logger.LogInformation(
                "YearEnd empty-table cleanup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | YearColumn={YearColumn} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
                context.CorrelationId,
                entry.Schema,
                entry.TableName,
                yearColumn,
                context.TargetFpsYear,
                deleted);
        }

        _logger.LogInformation(
            "YearEnd target-year empty-table cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);

        return context;
    }

    private static async Task<int> DeleteByYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string yearColumn,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"DELETE FROM {schema}.{table} WHERE {yearColumn} = @target_year;");
        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
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
