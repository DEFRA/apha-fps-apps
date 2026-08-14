using System.Data.Common;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Resets project financial fields for target-year project rows using strict year scoping.
/// </summary>
public sealed class ProjectFinancialResetStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyDictionary<string, string> ResetRules =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
        };

    private static readonly IReadOnlyList<ResetTarget> ResetTargets =
    [
        new("fps", "tlkpproject", "fpsyear"),
        new("mabarchive", "my_tlkpproject", "year")
    ];

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

        foreach (var target in ResetTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await YearEndSqlHelpers.TableExistsAsync(connection, transaction, target.Schema, target.Table, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd project reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.Table);
                continue;
            }

            var hasYearColumn = await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, target.Schema, target.Table, target.YearColumn, cancellationToken);
            if (!hasYearColumn)
            {
                throw new InvalidOperationException(
                    $"Table {target.Schema}.{target.Table} does not contain required year column {target.YearColumn} for safe reset.");
            }

            var setClauses = new List<string>();
            foreach (var rule in ResetRules)
            {
                if (await YearEndSqlHelpers.ColumnExistsAsync(connection, transaction, target.Schema, target.Table, rule.Key, cancellationToken))
                {
                    setClauses.Add($"{rule.Key} = {rule.Value}");
                }
            }

            if (setClauses.Count == 0)
            {
                _logger.LogWarning(
                    "YearEnd project reset found no resettable columns | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.Table);
                continue;
            }

            var updated = await ExecuteResetAsync(
                connection,
                transaction,
                target.Schema,
                target.Table,
                target.YearColumn,
                setClauses,
                context.TargetFpsYear.Value,
                cancellationToken);

            _logger.LogInformation(
                "YearEnd project financial reset completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
                context.CorrelationId,
                target.Schema,
                target.Table,
                context.TargetFpsYear,
                updated);
        }

        return context;
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

    private sealed record ResetTarget(string Schema, string Table, string YearColumn);
}
