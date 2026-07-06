using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

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

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<ProjectFinancialResetStep> _logger;

    public ProjectFinancialResetStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<ProjectFinancialResetStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ProjectFinancialResetStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before project financial reset.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        foreach (var target in ResetTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exists = await TableExistsAsync(connection, target.Schema, target.Table, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning(
                    "YearEnd project reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.Table);
                continue;
            }

            var hasYearColumn = await ColumnExistsAsync(connection, target.Schema, target.Table, target.YearColumn, cancellationToken);
            if (!hasYearColumn)
            {
                throw new InvalidOperationException(
                    $"Table {target.Schema}.{target.Table} does not contain required year column {target.YearColumn} for safe reset.");
            }

            var setClauses = new List<string>();
            foreach (var rule in ResetRules)
            {
                if (await ColumnExistsAsync(connection, target.Schema, target.Table, rule.Key, cancellationToken))
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

    private sealed record ResetTarget(string Schema, string Table, string YearColumn);
}