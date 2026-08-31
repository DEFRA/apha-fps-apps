using System.Collections.Generic;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Persistence contract for Year End Data Setup operations.
/// </summary>
public interface IYearEndDataSetupRepository
{
    /// <summary>
    /// Checks whether a year row exists in fps.tblyearmaster and returns its status and active flag,
    /// or null if no row exists for the given year.
    /// </summary>
    Task<(string YearStatus, bool Active)?> GetYearStateAsync(int fpsYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new row in fps.tblyearmaster with Planned status and returns the number of rows affected.
    /// </summary>
    Task<int> InsertPlannedYearAsync(int fpsYear, string fpsYearCode, string correlationId, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the specified table exists in information_schema.</summary>
    Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the specified column exists in information_schema.</summary>
    Task<bool> ColumnExistsAsync(string schema, string table, string column, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a row for the given fpsYear exists in fps.tblyearmaster.</summary>
    Task<bool> YearRowExistsAsync(int fpsYear, CancellationToken cancellationToken = default);

    /// <summary>Returns the count of rows in the given table matching the specified year column value.</summary>
    Task<long> CountRowsByYearAsync(string schema, string table, string yearColumn, int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the year column for a table by checking for "fpsyear" then "year" in information_schema.
    /// Returns null if neither column exists.
    /// </summary>
    Task<string?> ResolveYearColumnAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes target-year <c>fps.tblwgemployee</c> rows (and their dependent <c>fps.tblstaffjob</c>
    /// rows, deleted first for the FK) for employees who were inactive in the target year and are not
    /// the General Staff exemption, per the legacy <c>Annual_WGEmployeeList.sql</c> rule: inactive
    /// candidate is <c>personstatus = 'I'</c> (case-insensitive) AND <c>enddate IS NULL</c>; General
    /// Staff exemption (retained even if inactive) is <c>spnumber LIKE 'G%'</c> (case-sensitive) AND
    /// <c>UPPER(firstname) = 'GENERAL'</c>, both required. Any <c>personstatus</c> value other than
    /// <c>A</c>/<c>a</c>/<c>I</c>/<c>i</c> is a data-quality error and throws before any deletion.
    /// FPS-only — never touches <c>mabarchive</c>; MABArchive participation in Year End is gated
    /// exclusively through the dedicated MABArchive setup step. Returns the total rows deleted across
    /// both tables.
    /// </summary>
    Task<int> DeleteInactiveEmployeesForYearEndAsync(int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies fps.tblperiod rows from sourceYear into targetYear, using a dynamic column projection
    /// resolved from information_schema. Resets <c>periodlocked</c> and <c>finalsummariesrun</c> to
    /// <c>0</c> on the copied rows, rather than carrying the source year's lock/release state forward,
    /// and regenerates <c>periodname</c> for the target year instead of copying the source year's text.
    /// </summary>
    Task<int> CopyPeriodRowsAsync(int sourceYear, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies field reset rules to target-year rows in the specified table. Skips any column in
    /// <paramref name="rules"/> that doesn't exist on the table, rather than failing the whole
    /// operation — matrix entries carry the full override set for a reset phase and not every
    /// column applies to every table it's ever mixed with.
    /// </summary>
    Task<int> ResetFieldsByYearAsync(string schema, string table, string yearColumn, IReadOnlyDictionary<string, string> rules, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies fps-schema year-scoped rows from sourceYear into targetYear for the given table, using
    /// a dynamic column projection resolved from information_schema (excludes identity/generated
    /// columns, replaces the year column with the target year literal).
    /// </summary>
    Task<int> CopyFpsYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// True when <paramref name="table"/> is a native PostgreSQL partitioned table (<c>relkind = 'p'</c>).
    /// Distinct from <see cref="IsPartitionAttachedForYearAsync"/> so callers can tell "not partitioned
    /// at all" apart from "partitioned, but this year's partition is missing" — different failures with
    /// different fixes.
    /// </summary>
    Task<bool> IsPartitionedTableAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>
    /// True when <paramref name="table"/> (already confirmed partitioned via
    /// <see cref="IsPartitionedTableAsync"/>) has an explicit <c>LIST (fpsyear)</c> partition attached
    /// for <paramref name="year"/>. Deliberately does not match the catch-all <c>DEFAULT</c> partition —
    /// this answers "does this year have a dedicated partition," not "can this year's rows be inserted
    /// at all." Combine with <see cref="IsDefaultPartitionAttachedAsync"/> to decide overall routability.
    /// </summary>
    Task<bool> IsPartitionAttachedForYearAsync(string schema, string table, int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// True when <paramref name="table"/> (already confirmed partitioned via
    /// <see cref="IsPartitionedTableAsync"/>) has a <c>DEFAULT</c> partition attached — the catch-all
    /// destination for rows whose year doesn't match any explicit bound. A legitimate, DDL-free
    /// destination for a target year with no dedicated partition yet, at the cost of partition pruning.
    /// </summary>
    Task<bool> IsDefaultPartitionAttachedAsync(string schema, string table, CancellationToken cancellationToken = default);
}
