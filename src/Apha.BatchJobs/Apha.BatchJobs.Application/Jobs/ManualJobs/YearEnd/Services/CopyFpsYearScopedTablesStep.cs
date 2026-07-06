using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

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

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<CopyFpsYearScopedTablesStep> _logger;

    public CopyFpsYearScopedTablesStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<CopyFpsYearScopedTablesStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyFpsYearScopedTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before FPS year-scoped copy.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        foreach (var table in CopyTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await TableExistsAsync(connection, table, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table=fps.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            var hasFpsYearColumn = await ColumnExistsAsync(connection, table, "fpsyear", cancellationToken);
            if (!hasFpsYearColumn)
            {
                throw new InvalidOperationException($"Table fps.{table} does not contain fpsyear and cannot be copied safely.");
            }

            var targetRows = await CountRowsByYearAsync(connection, table, context.TargetFpsYear.Value, cancellationToken);
            if (targetRows > 0)
            {
                throw new InvalidOperationException(
                    $"Table fps.{table} already contains {targetRows} rows for target year {context.TargetFpsYear.Value}. Cleanup is required before Year End copy.");
            }

            var insertColumns = await GetInsertColumnsAsync(connection, table, cancellationToken);
            if (string.IsNullOrWhiteSpace(insertColumns))
            {
                throw new InvalidOperationException($"Could not resolve copyable columns for fps.{table}.");
            }

            var selectProjection = await GetSelectProjectionAsync(connection, table, cancellationToken);
            if (string.IsNullOrWhiteSpace(selectProjection))
            {
                throw new InvalidOperationException($"Could not resolve select projection for fps.{table}.");
            }

            var copied = await CopyRowsAsync(
                connection,
                table,
                insertColumns,
                selectProjection,
                context.CurrentFpsYear.Value,
                context.TargetFpsYear.Value,
                cancellationToken);

            var resetCount = await ApplyResetRulesAsync(
                connection,
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
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'fps'
                  AND table_name = @table_name
            );";

        AddParameter(command, "table_name", tableName);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'fps'
                  AND table_name = @table_name
                  AND column_name = @column_name
            );";

        AddParameter(command, "table_name", tableName);
        AddParameter(command, "column_name", columnName);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<long> CountRowsByYearAsync(DbConnection connection, string tableName, int fpsYear, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM fps.{tableName} WHERE fpsyear = @fpsyear;";
        AddParameter(command, "fpsyear", fpsYear);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    private static async Task<string?> GetInsertColumnsAsync(DbConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = 'fps'
              AND c.table_name = @table_name
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<string?> GetSelectProjectionAsync(DbConnection connection, string tableName, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
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
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "table_name", tableName);
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    private static async Task<int> CopyRowsAsync(
        DbConnection connection,
        string tableName,
        string insertColumns,
        string selectProjection,
        int currentFpsYear,
        int targetFpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO fps.{tableName} ({insertColumns})
            SELECT {selectProjection}
            FROM fps.{tableName}
            WHERE fpsyear = @current_fpsyear;";

        AddParameter(command, "current_fpsyear", currentFpsYear);
        AddParameter(command, "target_fpsyear", targetFpsYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<int> ApplyResetRulesAsync(
        DbConnection connection,
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
            var exists = await ColumnExistsAsync(connection, tableName, entry.Key, cancellationToken);
            if (exists)
            {
                setClauses.Add($"{entry.Key} = {entry.Value}");
            }
        }

        if (setClauses.Count == 0)
        {
            return 0;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            UPDATE fps.{tableName}
            SET {string.Join(", ", setClauses)}
            WHERE fpsyear = @target_fpsyear;";

        AddParameter(command, "target_fpsyear", targetFpsYear);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> ExecuteBooleanAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool value && value;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
