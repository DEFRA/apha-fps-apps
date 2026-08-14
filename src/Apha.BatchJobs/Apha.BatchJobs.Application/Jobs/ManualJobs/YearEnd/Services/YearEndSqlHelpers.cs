using System.Data.Common;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Shared ADO.NET helpers for Year End Data Setup/CutOver steps. Every command created through
/// these helpers is explicitly enlisted in the caller's shared transaction — no step may open its
/// own connection or transaction for Year End business work.
/// </summary>
internal static class YearEndSqlHelpers
{
    public static DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string commandText)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        return command;
    }

    public static async Task<bool> TableExistsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema
                  AND table_name = @table
            );");

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = @schema
                  AND table_name = @table
                  AND column_name = @column
            );");

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "column", column);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public static async Task<bool> ExecuteBooleanAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool value && value;
    }

    /// <summary>
    /// True when <paramref name="table"/> is a native PostgreSQL partitioned table
    /// (<c>relkind = 'p'</c>). Distinct from <see cref="IsPartitionAttachedForYearAsync"/> so
    /// callers can tell "not partitioned at all" apart from "partitioned, but this year's
    /// partition is missing" — different failures with different fixes.
    /// </summary>
    public static async Task<bool> IsPartitionedTableAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1 FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = @schema AND c.relname = @table AND c.relkind = 'p'
            );");

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    /// <summary>
    /// True when <paramref name="table"/> (already confirmed partitioned via
    /// <see cref="IsPartitionedTableAsync"/>) has an explicit <c>LIST (fpsyear)</c> partition
    /// attached for <paramref name="year"/>. Deliberately does not match the catch-all
    /// <c>_default</c> partition (its bound text is literally <c>DEFAULT</c>, not
    /// <c>FOR VALUES IN (...)</c>) — this method answers "does this year have a dedicated
    /// partition," not "can this year's rows be inserted at all." Callers combine this with
    /// <see cref="IsDefaultPartitionAttachedAsync"/> to decide overall routability — see that
    /// method's remarks for why both are legitimate, DDL-free routes.
    /// </summary>
    public static async Task<bool> IsPartitionAttachedForYearAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        int year,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1
                FROM pg_inherits i
                JOIN pg_class c ON c.oid = i.inhrelid
                JOIN pg_class p ON p.oid = i.inhparent
                JOIN pg_namespace n ON n.oid = p.relnamespace
                WHERE n.nspname = @schema
                  AND p.relname = @table
                  AND pg_get_expr(c.relpartbound, c.oid) = format('FOR VALUES IN (%s)', @year)
            );");

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "year", year);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    /// <summary>
    /// True when <paramref name="table"/> (already confirmed partitioned via
    /// <see cref="IsPartitionedTableAsync"/>) has a <c>DEFAULT</c> partition attached — the
    /// catch-all destination for rows whose <c>fpsyear</c> doesn't match any explicit
    /// <c>FOR VALUES IN (...)</c> bound. <c>fpsyear</c> is the authoritative business-year
    /// discriminator, not the physical partition — routing through <c>DEFAULT</c> is a legitimate,
    /// DDL-free destination for a target year with no dedicated partition yet, so a table with a
    /// <c>DEFAULT</c> partition attached can accept any year's rows without DBA involvement.
    /// Trade-off callers should be aware of: rows routed through <c>DEFAULT</c> lose partition
    /// pruning versus an explicit per-year partition, which matters most for
    /// high-volume/continuously-written tables (e.g. <c>timecodevalid</c>, <c>projectmonth2/3</c>)
    /// that other processes such as <c>RecreateSummaries</c> keep writing to all year, not just
    /// during Year End's own copy step.
    /// </summary>
    public static async Task<bool> IsDefaultPartitionAttachedAsync(
        DbConnection connection,
        DbTransaction transaction,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, @"
            SELECT EXISTS (
                SELECT 1
                FROM pg_inherits i
                JOIN pg_class c ON c.oid = i.inhrelid
                JOIN pg_class p ON p.oid = i.inhparent
                JOIN pg_namespace n ON n.oid = p.relnamespace
                WHERE n.nspname = @schema
                  AND p.relname = @table
                  AND pg_get_expr(c.relpartbound, c.oid) = 'DEFAULT'
            );");

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public static async Task<long> ExecuteCountAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    public static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
