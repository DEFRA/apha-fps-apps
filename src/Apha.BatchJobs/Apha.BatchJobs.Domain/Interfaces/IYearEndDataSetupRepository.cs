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
    /// Resolves the <c>fps.job_queue</c> row for a <c>YearEnd-DataSetup</c> request by its
    /// <c>jobexecutionid</c>, scoped to that job type via a join to <c>fps.job_master</c> so a
    /// <c>JobExecutionId</c> belonging to some other job never resolves as if it were a valid Data
    /// Setup request. Returns null if no matching row exists.
    /// </summary>
    Task<(Guid JobQueueId, int? TargetFpsYear)?> ResolveJobQueueByExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Materializes the Approve-frozen <c>fps.yearend_settings_staging</c> rows for
    /// <paramref name="jobQueueId"/> into <c>fps.tblsettings</c> for <paramref name="targetFpsYear"/>.
    /// Returns the number of rows inserted.
    /// </summary>
    Task<int> MaterializeStagedSettingsAsync(Guid jobQueueId, int targetFpsYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Materializes the Approve-frozen <c>fps.yearend_monthhours_staging</c> rows for
    /// <paramref name="jobQueueId"/> into <c>fps.tlkpmonthhours</c> for <paramref name="targetFpsYear"/>
    /// — <c>month_year</c> becomes <c>tlkpmonthhours.year</c>. Returns the number of rows inserted.
    /// </summary>
    Task<int> MaterializeStagedMonthHoursAsync(Guid jobQueueId, int targetFpsYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads <c>fps.tblsettings.setting</c> where <c>id = 'CapApprovalReceivedForReset'</c> for
    /// <paramref name="targetFpsYear"/>. Returns <c>null</c> if no such row exists — FPS is expected to
    /// guarantee the row exists with a valid <c>Yes</c>/<c>No</c> value before Year End Data Setup can be
    /// initiated or approved (<c>YearEndService.ValidateConfiguration</c>), so callers should treat
    /// <c>null</c> as a hard failure, not a default.
    /// </summary>
    Task<string?> GetCapApprovalReceivedForResetSettingAsync(int targetFpsYear, CancellationToken cancellationToken = default);
}
