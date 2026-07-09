/*
 * TRANSFORMENGINE MIGRATION — IYearlyFinancialDataRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# repository interface existed
 *   - Repository interface for YearlyFinancialData (my_tlkpprojectradtrackdata)
 *     and PactProjectYearCosts (vpactprojectyearcosts) operations
 *   - Composite key (year, project) used in GetByKeyAsync, UpdateAsync, DeleteAsync,
 *     ExistsAsync to match CONSTRAINT pk_my_tlkpprojectradtrackdata
 *   - Paginated list filtered by project to support the YearlyFinancialData grid
 *     (form frmProjectRadTrackData_Update displays all years for a given project)
 *   - GetPactCostsAsync returns aggregated view rows for the "Update Costing"
 *     button (btnUpdateCosting) — maps to vpactprojectyearcosts grouped by year
 *   - All signatures async-only — consistent with IProjectYearCostsRepository pattern
 *   - No DbContext, EF Core, or infrastructure types in this interface
 *
 * PRESERVED:
 *   - Method naming convention matches existing Core interfaces
 *     (e.g., IProjectDetailsRepository, IProjectYearCostsRepository)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether GetAllAsync needs additional filter parameters
 *     (e.g., year range, locked flag) beyond the project filter before the repository
 *     implementation is written in Phase 4
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for per-year financial data operations on
    /// <c>mabarchive.my_tlkpprojectradtrackdata</c> and the
    /// <c>mabarchive.vpactprojectyearcosts</c> read-only view.
    /// Composite primary key: (<see cref="YearlyFinancialData.Year"/>, <see cref="YearlyFinancialData.Project"/>).
    /// </summary>
    public interface IYearlyFinancialDataRepository
    {
        // TRANSFORMENGINE: Paginated list — filtered by project, supports search + sort + paging
        /// <summary>
        /// Returns a paginated list of yearly financial data records for the given project.
        /// </summary>
        /// <param name="project">Project code to filter by (FK to g_tlkpproject_radtrackdata.parentproject).</param>
        /// <param name="paging">Pagination, sorting, and search parameters.</param>
        Task<PagedData<YearlyFinancialData>> GetAllAsync(string project, PaginationParameters<string> paging);

        // TRANSFORMENGINE: Single record lookup by composite key (year + project)
        /// <summary>
        /// Returns a single yearly financial data record identified by the composite key,
        /// or <c>null</c> if not found.
        /// </summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar(20)).</param>
        Task<YearlyFinancialData?> GetByKeyAsync(short year, string project);

        // TRANSFORMENGINE: Existence check — AnyAsync-style semantics for validation
        /// <summary>
        /// Returns <c>true</c> if a record with the given composite key already exists.
        /// </summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar(20)).</param>
        Task<bool> ExistsAsync(short year, string project);

        // TRANSFORMENGINE: Create — inserts a new row into my_tlkpprojectradtrackdata
        /// <summary>
        /// Creates a new yearly financial data record and returns the persisted entity.
        /// </summary>
        /// <param name="entity">Populated entity to insert.</param>
        Task<YearlyFinancialData> CreateAsync(YearlyFinancialData entity);

        // TRANSFORMENGINE: Update — updates an existing row identified by composite key
        /// <summary>
        /// Updates an existing yearly financial data record and returns the updated entity.
        /// </summary>
        /// <param name="entity">Entity containing updated values; Year and Project identify the row.</param>
        Task<YearlyFinancialData> UpdateAsync(YearlyFinancialData entity);

        // TRANSFORMENGINE: Delete — removes a row identified by composite key
        /// <summary>
        /// Deletes the yearly financial data record for the given composite key.
        /// Returns <c>true</c> if the row was found and deleted; <c>false</c> if not found.
        /// </summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar(20)).</param>
        Task<bool> DeleteAsync(short year, string project);

        // TRANSFORMENGINE: PACT actuals query — reads from vpactprojectyearcosts view
        //   Used by the "Update Costing" button (btnUpdateCosting) to populate actual
        //   spend values back into the yearly financial data record
        /// <summary>
        /// Returns aggregated PACT actual cost rows from the <c>vpactprojectyearcosts</c> view
        /// for the given project and year.
        /// </summary>
        /// <param name="project">Project code to filter by.</param>
        /// <param name="year">Financial year to filter by.</param>
        Task<IReadOnlyList<PactProjectYearCosts>> GetPactCostsAsync(string project, short year);
    }
}
