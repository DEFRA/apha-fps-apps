using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies configured planning-field resets for target-year rows using strict year scoping.
/// </summary>
public sealed class ConfiguredPlanningResetStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FpsResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
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

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MabArchiveResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["my_tblstaffjob"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["plannedhours"] = "0"
            },
            ["my_tlkptestreqmt"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["norequired"] = "0"
            },
            ["my_tblanimalreq"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["numberofanimals"] = "0",
                ["numberofdays"] = "0"
            },
            ["my_tbladditionalcosts"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["itemcost"] = "0"
            }
        };

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<ConfiguredPlanningResetStep> _logger;

    public ConfiguredPlanningResetStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<ConfiguredPlanningResetStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
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

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        var totalUpdated = 0;

        totalUpdated += await ApplyResetsForSchemaAsync(
            connection,
            schema: "fps",
            yearColumn: "fpsyear",
            tableRules: FpsResetRulesByTable,
            targetYear: context.TargetFpsYear.Value,
            correlationId: context.CorrelationId,
            cancellationToken: cancellationToken);

        totalUpdated += await ApplyResetsForSchemaAsync(
            connection,
            schema: "mabarchive",
            yearColumn: "year",
            tableRules: MabArchiveResetRulesByTable,
            targetYear: context.TargetFpsYear.Value,
            correlationId: context.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "YearEnd configured planning reset completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalUpdated);
    }

    private async Task<int> ApplyResetsForSchemaAsync(
        DbConnection connection,
        string schema,
        string yearColumn,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> tableRules,
        int targetYear,
        string correlationId,
        CancellationToken cancellationToken)
    {
        var updatedTotal = 0;

        foreach (var tableEntry in tableRules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = tableEntry.Key;
            var resetRules = tableEntry.Value;

            var exists = await TableExistsAsync(connection, schema, tableName, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd planning reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    correlationId,
                    schema,
                    tableName);
                continue;
            }

            var hasYearColumn = await ColumnExistsAsync(connection, schema, tableName, yearColumn, cancellationToken);
            if (!hasYearColumn)
            {
                throw new InvalidOperationException(
                    $"Table {schema}.{tableName} does not contain required year column {yearColumn} for safe planning reset.");
            }

            var setClauses = new List<string>();
            foreach (var rule in resetRules)
            {
                if (await ColumnExistsAsync(connection, schema, tableName, rule.Key, cancellationToken))
                {
                    setClauses.Add($"{rule.Key} = {rule.Value}");
                }
            }

            if (setClauses.Count == 0)
            {
                _logger.LogWarning(
                    "YearEnd planning reset found no resettable columns | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    correlationId,
                    schema,
                    tableName);
                continue;
            }

            var updated = await ExecuteResetAsync(
                connection,
                schema,
                tableName,
                yearColumn,
                setClauses,
                targetYear,
                cancellationToken);

            updatedTotal += updated;

            _logger.LogInformation(
                "YearEnd planning reset completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
                correlationId,
                schema,
                tableName,
                targetYear,
                updated);
        }

        return updatedTotal;
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema_name
                  AND table_name = @table_name
            );";

        AddParameter(command, "schema_name", schema);
        AddParameter(command, "table_name", table);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        string schema,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = @schema_name
                  AND table_name = @table_name
                  AND column_name = @column_name
            );";

        AddParameter(command, "schema_name", schema);
        AddParameter(command, "table_name", table);
        AddParameter(command, "column_name", column);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    private static async Task<int> ExecuteResetAsync(
        DbConnection connection,
        string schema,
        string table,
        string yearColumn,
        IReadOnlyList<string> setClauses,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            UPDATE {schema}.{table}
            SET {string.Join(", ", setClauses)}
            WHERE {yearColumn} = @target_year;";

        AddParameter(command, "target_year", targetYear);
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
