using System.Data.Common;
using Microsoft.Extensions.Logging;

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

    private readonly ILogger<ConfiguredPlanningResetStep> _logger;

    public ConfiguredPlanningResetStep(ILogger<ConfiguredPlanningResetStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConfiguredPlanningResetStep";

    public async Task<YearEndExecutionContext> ExecuteAsync(
        YearEndExecutionContext context,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before configured planning reset.");
        }

        var totalUpdated = 0;

        totalUpdated += await ApplyResetsForSchemaAsync(
            connection,
            transaction,
            schema: "fps",
            yearColumn: "fpsyear",
            tableRules: FpsResetRulesByTable,
            targetYear: context.TargetFpsYear.Value,
            correlationId: context.CorrelationId,
            cancellationToken: cancellationToken);

        totalUpdated += await ApplyResetsForSchemaAsync(
            connection,
            transaction,
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

        return context;
    }

    private async Task<int> ApplyResetsForSchemaAsync(
        DbConnection connection,
        DbTransaction transaction,
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

            var exists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, schema, tableName, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd planning reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    correlationId,
                    schema,
                    tableName);
                continue;
            }

            var hasYearColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, tableName, yearColumn, cancellationToken);
            if (!hasYearColumn)
            {
                throw new InvalidOperationException(
                    $"Table {schema}.{tableName} does not contain required year column {yearColumn} for safe planning reset.");
            }

            var setClauses = new List<string>();
            foreach (var rule in resetRules)
            {
                if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, schema, tableName, rule.Key, cancellationToken))
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
                transaction,
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

    private static async Task<int> ExecuteResetAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string yearColumn,
        IReadOnlyList<string> setClauses,
        int targetYear,
        CancellationToken cancellationToken)
    {
        await using var command = YearEndSqlHelpers.CreateCommand(connection, transaction, $@"
            UPDATE {schema}.{table}
            SET {string.Join(", ", setClauses)}
            WHERE {yearColumn} = @target_year;");

        YearEndSqlHelpers.AddParameter(command, "target_year", targetYear);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
