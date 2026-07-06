using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Removes target-year staff-job rows that map to inactive employees when inactive markers are available.
/// </summary>
public sealed class InactiveEmployeeCleanupStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<CleanupTarget> Targets =
    [
        new("fps", "tblstaffjob", "fpsyear", "staffid", "tblwgemployee", "pactid"),
        new("mabarchive", "my_tblstaffjob", "year", "staffid", "my_tblwgemployee", "pactid")
    ];

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<InactiveEmployeeCleanupStep> _logger;

    public InactiveEmployeeCleanupStep(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<InactiveEmployeeCleanupStep> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "InactiveEmployeeCleanupStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before inactive employee cleanup.");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        var totalDeleted = 0;

        foreach (var target in Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deleted = await CleanupTargetAsync(connection, target, context, cancellationToken);
            totalDeleted += deleted;
        }

        _logger.LogInformation(
            "YearEnd inactive employee cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);
    }

    private async Task<int> CleanupTargetAsync(
        DbConnection connection,
        CleanupTarget target,
        YearEndExecutionContext context,
        CancellationToken cancellationToken)
    {
        var jobTableExists = await TableExistsAsync(connection, target.Schema, target.JobTable, cancellationToken);
        if (!jobTableExists)
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped missing job table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.JobTable);
            return 0;
        }

        var employeeTableExists = await TableExistsAsync(connection, target.Schema, target.EmployeeTable, cancellationToken);
        if (!employeeTableExists)
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped missing employee table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.EmployeeTable);
            return 0;
        }

        var hasYearColumn = await ColumnExistsAsync(connection, target.Schema, target.JobTable, target.YearColumn, cancellationToken);
        var hasStaffColumn = await ColumnExistsAsync(connection, target.Schema, target.JobTable, target.JobStaffColumn, cancellationToken);
        var hasEmployeeStaffColumn = await ColumnExistsAsync(connection, target.Schema, target.EmployeeTable, target.EmployeeStaffColumn, cancellationToken);

        if (!hasYearColumn || !hasStaffColumn || !hasEmployeeStaffColumn)
        {
            throw new InvalidOperationException(
                $"Inactive cleanup cannot run safely for {target.Schema}.{target.JobTable}; required columns are missing.");
        }

        var inactivePredicate = await BuildInactivePredicateAsync(connection, target.Schema, target.EmployeeTable, cancellationToken);
        if (string.IsNullOrWhiteSpace(inactivePredicate))
        {
            _logger.LogWarning(
                "YearEnd inactive cleanup skipped because no inactive markers were found | CorrelationId={CorrelationId} | EmployeeTable={Schema}.{Table}",
                context.CorrelationId,
                target.Schema,
                target.EmployeeTable);
            return 0;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            DELETE FROM {target.Schema}.{target.JobTable} sj
            WHERE sj.{target.YearColumn} = @target_year
              AND EXISTS (
                    SELECT 1
                    FROM {target.Schema}.{target.EmployeeTable} e
                    WHERE e.{target.EmployeeStaffColumn} = sj.{target.JobStaffColumn}
                      AND ({inactivePredicate})
              );";

        AddParameter(command, "target_year", context.TargetFpsYear!.Value);
        var deleted = await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation(
            "YearEnd inactive cleanup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            target.Schema,
            target.JobTable,
            context.TargetFpsYear,
            deleted);

        return deleted;
    }

    private static async Task<string?> BuildInactivePredicateAsync(
        DbConnection connection,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        var predicates = new List<string>();

        if (await ColumnExistsAsync(connection, schema, table, "active", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.active as text), '')) in ('false', '0', 'n', 'no')");
        }

        if (await ColumnExistsAsync(connection, schema, table, "isactive", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.isactive as text), '')) in ('false', '0', 'n', 'no')");
        }

        if (await ColumnExistsAsync(connection, schema, table, "employmentstatus", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.employmentstatus as text), '')) in ('inactive', 'leaver', 'left', 'terminated')");
        }

        if (await ColumnExistsAsync(connection, schema, table, "status", cancellationToken))
        {
            predicates.Add("lower(coalesce(cast(e.status as text), '')) in ('inactive', 'leaver', 'left', 'terminated')");
        }

        return predicates.Count == 0 ? null : string.Join(" OR ", predicates);
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

    private sealed record CleanupTarget(
        string Schema,
        string JobTable,
        string YearColumn,
        string JobStaffColumn,
        string EmployeeTable,
        string EmployeeStaffColumn);
}
