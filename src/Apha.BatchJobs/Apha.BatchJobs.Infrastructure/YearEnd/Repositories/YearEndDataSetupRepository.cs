using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Apha.BatchJobs.Infrastructure.YearEnd.Repositories;

/// <summary>
/// Executes Year End Data Setup persistence operations against fps.tblyearmaster.
/// </summary>
/// <remarks>
/// Takes a **scoped** <see cref="BatchJobsDbContext"/> (not <see cref="Microsoft.EntityFrameworkCore.IDbContextFactory{TContext}"/>)
/// so every method call within one job execution's DI scope shares the same connection. This is
/// what lets <see cref="YearEndDataSetupTransactionManager"/> wrap the entire 11-step pipeline in one
/// ambient transaction (main-port Phase 7A, 2026-08-28) —
/// no method here may open its own <c>IDbContextFactory</c>-created context, or it would silently
/// escape that transaction and break the all-or-nothing guarantee. Enforced by
/// <c>YearEndDataSetupRepositoryUsesSharedContextTests</c>.
/// </remarks>
public sealed class YearEndDataSetupRepository : IYearEndDataSetupRepository
{
    private const string PlannedStatus = "Planned";
    private const string BatchCreatedBy = "YearEndBatchWorker";

    private readonly BatchJobsDbContext _context;

    public YearEndDataSetupRepository(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
        }

        return connection;
    }

    public async Task<(string YearStatus, bool Active)?> GetYearStateAsync(int fpsYear, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear;";

        AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return (yearStatus, active);
    }

    public async Task<int> InsertPlannedYearAsync(int fpsYear, string fpsYearCode, string correlationId, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES (@fpsyear, @fpsyearcode, @yearstatus, @remarks, @active, @createdby);";

        AddParameter(command, "fpsyear", fpsYear);
        AddParameter(command, "fpsyearcode", fpsYearCode);
        AddParameter(command, "yearstatus", PlannedStatus);
        AddParameter(command, "remarks", $"Created by YearEndDataSetup. CorrelationId={correlationId}");
        AddParameter(command, "active", true);
        AddParameter(command, "createdby", BatchCreatedBy);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema
                  AND table_name = @table
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> ColumnExistsAsync(string schema, string table, string column, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = @schema
                  AND table_name = @table
                  AND column_name = @column
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "column", column);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> YearRowExistsAsync(int fpsYear, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM fps.tblyearmaster ym
                WHERE ym.fpsyear = @fpsyear
            );";

        AddParameter(command, "fpsyear", fpsYear);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<long> CountRowsByYearAsync(string schema, string table, string yearColumn, int year, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @target_year;";
        AddParameter(command, "target_year", year);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    public async Task<string?> ResolveYearColumnAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        if (await ColumnExistsAsync(schema, table, "fpsyear", cancellationToken))
        {
            return "fpsyear";
        }

        if (await ColumnExistsAsync(schema, table, "year", cancellationToken))
        {
            return "year";
        }

        return null;
    }

    public async Task<int> DeleteInactiveEmployeesForYearEndAsync(int targetYear, CancellationToken cancellationToken = default)
    {
        const string schema = "fps";

        var connection = await GetOpenConnectionAsync(cancellationToken);

        await ValidatePersonStatusValuesAsync(connection, schema, targetYear, cancellationToken);

        var eligiblePactIds = await GetInactiveNonGeneralStaffPactIdsAsync(connection, schema, targetYear, cancellationToken);
        if (eligiblePactIds.Count == 0)
        {
            return 0;
        }

        // tblstaffjob has an FK to tblwgemployee — dependent rows must go first.
        var staffJobDeleted = await DeleteByKeyValuesAsync(connection, schema, "tblstaffjob", "staffid", "fpsyear", targetYear, eligiblePactIds, cancellationToken);
        var wgEmployeeDeleted = await DeleteByKeyValuesAsync(connection, schema, "tblwgemployee", "pactid", "fpsyear", targetYear, eligiblePactIds, cancellationToken);

        return staffJobDeleted + wgEmployeeDeleted;
    }

    /// <summary>
    /// Data-quality gate, run before any deletion: every target-year <c>personstatus</c> value must
    /// be <c>A</c>/<c>a</c>/<c>I</c>/<c>i</c>. Anything else is ambiguous and must block cleanup
    /// entirely rather than be silently classified either way.
    /// </summary>
    private static async Task ValidatePersonStatusValuesAsync(DbConnection connection, string schema, int targetYear, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT pactid, personstatus
            FROM {schema}.tblwgemployee
            WHERE fpsyear = @target_year
              AND UPPER(personstatus) NOT IN ('A', 'I')
            ORDER BY pactid
            LIMIT 20;";

        AddParameter(command, "target_year", targetYear);

        var unexpected = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var pactid = reader.IsDBNull(0) ? "<null>" : reader.GetString(0);
                var personStatus = reader.IsDBNull(1) ? "<null>" : reader.GetString(1);
                unexpected.Add($"{pactid}='{personStatus}'");
            }
        }

        if (unexpected.Count > 0)
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} has {schema}.tblwgemployee rows with an unexpected personstatus value " +
                $"(expected only A/a/I/i): {string.Join(", ", unexpected)}. Resolve the data quality issue before Year End cleanup can proceed.");
        }
    }

    /// <summary>
    /// Target-year <c>pactid</c> values for employees who are inactive (<c>personstatus='I'</c>
    /// case-insensitive, <c>enddate IS NULL</c>) and not the General Staff exemption
    /// (<c>spnumber LIKE 'G%' AND UPPER(firstname)='GENERAL'</c>, both required). The join to
    /// <c>tblemployee</c> is year-scoped and inner — an employee with no matching target-year
    /// <c>tblemployee</c> row is never treated as eligible for removal.
    /// </summary>
    private static async Task<IReadOnlyList<string>> GetInactiveNonGeneralStaffPactIdsAsync(DbConnection connection, string schema, int targetYear, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT wg.pactid
            FROM {schema}.tblwgemployee wg
            JOIN {schema}.tblemployee e
              ON e.spnumber = wg.spnumber
             AND e.fpsyear = wg.fpsyear
            WHERE wg.fpsyear = @target_year
              AND UPPER(wg.personstatus) = 'I'
              AND wg.enddate IS NULL
              AND NOT (wg.spnumber LIKE 'G%' AND UPPER(TRIM(e.firstname)) = 'GENERAL');";

        AddParameter(command, "target_year", targetYear);

        var pactIds = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pactIds.Add(reader.GetString(0));
        }

        return pactIds;
    }

    private static async Task<int> DeleteByKeyValuesAsync(DbConnection connection, string schema, string table, string keyColumn, string yearColumn, int targetYear, IReadOnlyList<string> keyValues, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            DELETE FROM {schema}.{table}
            WHERE {yearColumn} = @target_year
              AND {keyColumn} = ANY(@key_values);";

        AddParameter(command, "target_year", targetYear);
        AddParameter(command, "key_values", keyValues.ToArray());

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> CopyPeriodRowsAsync(int sourceYear, int targetYear, CancellationToken cancellationToken = default)
    {
        const string schema = "fps";
        const string table = "tblperiod";
        var resetToZeroColumns = new[] { "periodlocked", "finalsummariesrun" };

        var connection = await GetOpenConnectionAsync(cancellationToken);

        var insertColumns = await GetCopyableColumnsAsync(connection, schema, table, cancellationToken, forSelect: false);
        if (string.IsNullOrWhiteSpace(insertColumns))
        {
            throw new InvalidOperationException($"Could not resolve copyable columns for {schema}.{table}.");
        }

        var selectProjection = await GetPeriodSelectProjectionAsync(connection, schema, table, resetToZeroColumns, cancellationToken);
        if (string.IsNullOrWhiteSpace(selectProjection))
        {
            throw new InvalidOperationException($"Could not resolve select projection for {schema}.{table}.");
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO {schema}.{table} ({insertColumns})
            SELECT {selectProjection}
            FROM {schema}.{table}
            WHERE fpsyear = @source_year;";

        AddParameter(command, "source_year", sourceYear);
        AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Builds the tblperiod SELECT projection: <c>fpsyear</c> becomes the target year,
    /// <paramref name="resetToZeroColumns"/> (periodlocked/finalsummariesrun — nothing locked or
    /// released yet in a brand-new year) reset to <c>0</c>, <c>periodname</c> is regenerated for the
    /// target year (see <see cref="PeriodNameExpression"/>) instead of carried over from the source
    /// year's text, and every other column passes through unchanged.
    /// </summary>
    private static async Task<string?> GetPeriodSelectProjectionAsync(DbConnection connection, string schema, string table, IReadOnlyCollection<string> resetToZeroColumns, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT string_agg(
                CASE
                    WHEN c.column_name = 'fpsyear' THEN '@target_year AS fpsyear'
                    WHEN c.column_name = 'periodname' THEN @periodname_expression || ' AS periodname'
                    WHEN c.column_name = ANY(@reset_to_zero_columns) THEN '0 AS ' || format('%I', c.column_name)
                    ELSE format('%I', c.column_name)
                END,
                ', ' ORDER BY c.ordinal_position)
            FROM information_schema.columns c
            WHERE c.table_schema = @schema
              AND c.table_name = @table
              AND COALESCE(c.is_identity, 'NO') = 'NO'
              AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "reset_to_zero_columns", resetToZeroColumns.ToArray());
        AddParameter(command, "periodname_expression", PeriodNameExpression);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    /// <summary>
    /// Target-year <c>fps.tblperiod.periodname</c> text, keyed off the source row's own
    /// <c>endperiod</c> (1-12, cumulative month count from April). Reverse-engineered from the
    /// legacy wording still intact in FY2016-2018/2021 (e.g. "April - May 2016/17", "Year Total
    /// 2016/17" — note the double space before the year). Several later years show drifted text
    /// (e.g. "April - May 2023/23" instead of "2023/24") that is exactly the symptom of this column
    /// having previously been carried over unchanged instead of regenerated per target year — this
    /// expression is a best-effort fix, not verified against a documented legacy spec (none exists
    /// in this repo). <c>@target_year</c> resolves against the caller's own parameter of that name.
    /// </summary>
    private const string PeriodNameExpression = @"
        CASE
            WHEN endperiod = 1 THEN 'April ' || @target_year::text || ' Only'
            WHEN endperiod = 2 THEN 'April - May ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 3 THEN 'April - June ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 4 THEN 'April - July ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 5 THEN 'April - August ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 6 THEN 'April - September ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 7 THEN 'April - October ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 8 THEN 'April - November ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 9 THEN 'April - December ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            WHEN endperiod = 10 THEN 'April ' || @target_year::text || ' - January ' || (@target_year + 1)::text
            WHEN endperiod = 11 THEN 'April ' || @target_year::text || ' - February ' || (@target_year + 1)::text
            WHEN endperiod = 12 THEN 'Year Total  ' || @target_year::text || '/' || lpad(((@target_year + 1) % 100)::text, 2, '0')
            ELSE periodname
        END";

    public async Task<int> ResetFieldsByYearAsync(string schema, string table, string yearColumn, IReadOnlyDictionary<string, string> rules, int targetYear, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        var setClauses = new List<string>();
        foreach (var (column, value) in rules)
        {
            if (await ColumnExistsAsync(schema, table, column, cancellationToken))
            {
                setClauses.Add($"{column} = {value}");
            }
        }

        if (setClauses.Count == 0)
        {
            return 0;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            UPDATE {schema}.{table}
            SET {string.Join(", ", setClauses)}
            WHERE {yearColumn} = @target_year;";

        AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> CopyFpsYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default)
    {
        const string schema = "fps";

        var connection = await GetOpenConnectionAsync(cancellationToken);

        var insertColumns = await GetCopyableColumnsAsync(connection, schema, table, cancellationToken, forSelect: false);
        if (string.IsNullOrWhiteSpace(insertColumns))
        {
            throw new InvalidOperationException($"Could not resolve copyable columns for {schema}.{table}.");
        }

        var selectProjection = await GetCopyableColumnsAsync(connection, schema, table, cancellationToken, forSelect: true);
        if (string.IsNullOrWhiteSpace(selectProjection))
        {
            throw new InvalidOperationException($"Could not resolve select projection for {schema}.{table}.");
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            INSERT INTO {schema}.{table} ({insertColumns})
            SELECT {selectProjection}
            FROM {schema}.{table}
            WHERE fpsyear = @source_year;";

        AddParameter(command, "source_year", sourceYear);
        AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves the non-identity, non-generated columns for <paramref name="table"/> from
    /// information_schema. When <paramref name="forSelect"/> is true, the <c>fpsyear</c> column is
    /// projected as the <c>@target_year</c> parameter instead of its own name, so the same column set
    /// drives both the INSERT column list and the SELECT projection in <see cref="CopyFpsYearScopedTableAsync"/>.
    /// </summary>
    private static async Task<string?> GetCopyableColumnsAsync(DbConnection connection, string schema, string table, CancellationToken cancellationToken, bool forSelect)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = forSelect
            ? @"
                SELECT string_agg(
                    CASE
                        WHEN c.column_name = 'fpsyear' THEN '@target_year AS fpsyear'
                        ELSE format('%I', c.column_name)
                    END,
                    ', ' ORDER BY c.ordinal_position)
                FROM information_schema.columns c
                WHERE c.table_schema = @schema
                  AND c.table_name = @table
                  AND COALESCE(c.is_identity, 'NO') = 'NO'
                  AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';"
            : @"
                SELECT string_agg(format('%I', c.column_name), ', ' ORDER BY c.ordinal_position)
                FROM information_schema.columns c
                WHERE c.table_schema = @schema
                  AND c.table_name = @table
                  AND COALESCE(c.is_identity, 'NO') = 'NO'
                  AND COALESCE(c.is_generated, 'NEVER') = 'NEVER';";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar?.ToString();
    }

    public async Task<bool> IsPartitionedTableAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1 FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = @schema AND c.relname = @table AND c.relkind = 'p'
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> IsPartitionAttachedForYearAsync(string schema, string table, int year, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM pg_inherits i
                JOIN pg_class c ON c.oid = i.inhrelid
                JOIN pg_class p ON p.oid = i.inhparent
                JOIN pg_namespace n ON n.oid = p.relnamespace
                WHERE n.nspname = @schema
                  AND p.relname = @table
                  AND pg_get_expr(c.relpartbound, c.oid) = format('FOR VALUES IN (%s)', @year)
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "year", year);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> IsDefaultPartitionAttachedAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM pg_inherits i
                JOIN pg_class c ON c.oid = i.inhrelid
                JOIN pg_class p ON p.oid = i.inhparent
                JOIN pg_namespace n ON n.oid = p.relnamespace
                WHERE n.nspname = @schema
                  AND p.relname = @table
                  AND pg_get_expr(c.relpartbound, c.oid) = 'DEFAULT'
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);

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
}
