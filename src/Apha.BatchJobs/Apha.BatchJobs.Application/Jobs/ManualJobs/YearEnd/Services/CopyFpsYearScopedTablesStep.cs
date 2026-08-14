using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Copies vetted fps schema tables from current FPS year into target FPS year using strict year scoping.
/// </summary>
public sealed class CopyFpsYearScopedTablesStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<string> CopyTables =
    [
        "tlkpproject",
        "tblstaffjob",
        "tlkptestreqmt",
        "tblanimalreq",
        "tbladditionalcosts"
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["tlkpproject"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["transferincome"] = "0",
                ["custincome"] = "0",
                ["wip_eoy"] = "0",
                ["feccost"] = "0",
                ["profit"] = "0",
                ["budget_cvl"] = "0",
                ["carryover"] = "0",
                ["wip_limit"] = "NULL",
                ["wip_current"] = "NULL",
                ["pvsincome"] = "NULL",
                ["plancaseworkdebit"] = "NULL"
            },
            ["tblstaffjob"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["plannedhours"] = "0"
            },
            ["tlkptestreqmt"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["norequired"] = "0"
            },
            ["tblanimalreq"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["numberofanimals"] = "0",
                ["numberofdays"] = "0"
            },
            ["tbladditionalcosts"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["itemcost"] = "0"
            }
        };

    private readonly ILogger<CopyFpsYearScopedTablesStep> _logger;

    public CopyFpsYearScopedTablesStep(ILogger<CopyFpsYearScopedTablesStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyFpsYearScopedTablesStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before FPS year-scoped copy.");
        }

        foreach (var table in CopyTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, "fps", table, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table=fps.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            var hasFpsYearColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, "fps", table, "fpsyear", cancellationToken);
            if (!hasFpsYearColumn)
            {
                throw new InvalidOperationException($"Table fps.{table} does not contain fpsyear and cannot be copied safely.");
            }

            var targetRows = await CountRowsByYearAsync(connection, transaction, table, context.TargetFpsYear.Value, cancellationToken);
            if (targetRows > 0)
            {
                throw new InvalidOperationException(
                    $"Table fps.{table} already contains {targetRows} rows for target year {context.TargetFpsYear.Value}. Cleanup is required before Year End copy.");
            }

            var insertColumns = await GetInsertColumnsAsync(connection, transaction, table, cancellationToken);
            if (string.IsNullOrWhiteSpace(insertColumns))
            {
                throw new InvalidOperationException($"Could not resolve copyable columns for fps.{table}.");
            }

            var selectProjection = await GetSelectProjectionAsync(connection, transaction, table, cancellationToken);
            if (string.IsNullOrWhiteSpace(selectProjection))
            {
                throw new InvalidOperationException($"Could not resolve select projection for fps.{table}.");
            }

            var copied = await CopyRowsAsync(
                connection,
                transaction,
                table,
                insertColumns,
                selectProjection,
                context.CurrentFpsYear.Value,
                context.TargetFpsYear.Value,
                cancellationToken);

            var resetCount = await ApplyResetRulesAsync(
                connection,
                transaction,
                table,
                context.TargetFpsYear.Value,
                cancellationToken);

            _logger.LogInformation(
                "YearEnd table copy completed | CorrelationId={CorrelationId} | Table=fps.{Table} | SourceYear={SourceYear} | TargetYear={TargetYear} | CopiedRows={CopiedRows} | ResetRows={ResetRows}",
                context.CorrelationId,
                table,
                context.CurrentFpsYear,
                context.TargetFpsYear,
                copied,
                resetCount);
        }

        return context;
    }

    private static async Task<long> CountRowsByYearAsync(DbConnection connection, DbTransaction transaction, string tableName, int fpsYear, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $"SELECT COUNT(*) FROM fps.{tableName} WHERE fpsyear = @fpsyear;");
        YearEndSqlHelpers.AddParameter(command, "fpsyear", fpsYear);
        return await YearEndSqlHelpers.ExecuteCountAsync(command, cancellationToken);
    }

    private static async Task<string?> GetInsertColumnsAsync(DbConnection connection, DbTransaction transaction, string tableName, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = 'fps'
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<string?> GetSelectProjectionAsync(DbConnection connection, DbTransaction transaction, string tableName, CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, @"
            SELECT string_agg(
                CASE
                    WHEN c.column_name = 'fpsyear' THEN '@target_fpsyear AS fpsyear'
                    ELSE format('%I', c.column_name)
                END,
                ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = 'fps'
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';");

        YearEndSqlHelpers.AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<int> CopyRowsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string tableName,
        string insertColumns,
        string selectProjection,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            INSERT INTO fps.{tableName} ({insertColumns})
            SELECT {selectProjection}
            FROM fps.{tableName}
            WHERE fpsyear = @current_fpsyear;");

        YearEndSqlHelpers.AddParameter(command, "current_fpsyear", currentFpsYear);
        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> ApplyResetRulesAsync(
        DbConnection connection,
        DbTransaction transaction,
        string tableName,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        if (!ResetRulesByTable.TryGetValue(tableName, out var resetRules) || resetRules.Count == 0)
        {
            return 0;
        }

        var setClauses = new List<string>();
        foreach (var entry in resetRules)
        {
            var exists = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, "fps", tableName, entry.Key, cancellationToken);
            if (exists)
            {
                setClauses.Add($"{entry.Key} = {entry.Value}");
            }
        }

        if (setClauses.Count == 0)
        {
            return 0;
        }

        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            UPDATE fps.{tableName}
            SET {string.Join(", ", setClauses)}
            WHERE fpsyear = @target_fpsyear;");

        YearEndSqlHelpers.AddParameter(command, "target_fpsyear", targetFpsYear);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
