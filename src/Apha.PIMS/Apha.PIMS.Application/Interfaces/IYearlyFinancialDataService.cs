/*
 * TRANSFORMENGINE MIGRATION — IYearlyFinancialDataService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# service interface existed
 *   - Backend application service interface for YearlyFinancialData (my_tlkpprojectradtrackdata)
 *     and PactProjectYearCosts (vpactprojectyearcosts) orchestration
 *   - Composite key (year, project) used in GetByKeyAsync, UpdateAsync, DeleteAsync,
 *     ExistsAsync — matches IYearlyFinancialDataRepository signature
 *   - GetAllAsync accepts QueryParameters<string> (project code as filter) to
 *     support paginated grid for all years of a given project
 *   - GetPactCostsAsync maps to vpactprojectyearcosts query driven by btnUpdateCosting
 *   - All signatures async-only — consistent with IRadTrackInvoiceService pattern
 *
 * PRESERVED:
 *   - Method naming convention matches existing Application service interfaces
 *   - Parameter ordering matches IYearlyFinancialDataRepository where applicable
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether additional filter parameters are needed
 *     on GetAllAsync (e.g. year range, locked flag) beyond the project code filter
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    /// <summary>
    /// Application service interface for yearly financial data operations.
    /// Orchestrates <see cref="Apha.PIMS.Core.Interfaces.IYearlyFinancialDataRepository"/> calls
    /// and preserves business logic extracted from VBA and stored procedure guards.
    /// </summary>
    public interface IYearlyFinancialDataService
    {
        // TRANSFORMENGINE: Paginated list — filtered by project code; maps to form's RecordSource filter
        /// <summary>
        /// Returns a paginated list of yearly financial data records for the given project.
        /// </summary>
        /// <param name="parameters">Pagination, sorting, and search parameters; Filter contains the project code.</param>
        Task<PaginatedResult<YearlyFinancialDataDto>> GetAllAsync(QueryParameters<string> parameters);

        // TRANSFORMENGINE: Single record lookup by composite key (year + project)
        /// <summary>
        /// Returns a single yearly financial data DTO identified by the composite key,
        /// or <c>null</c> if not found.
        /// </summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar(20)).</param>
        Task<YearlyFinancialDataDto?> GetByKeyAsync(short year, string project);

        // TRANSFORMENGINE: Create — inserts new row into my_tlkpprojectradtrackdata;
        //   validates no duplicate composite key before insert
        /// <summary>
        /// Creates a new yearly financial data record and returns the persisted DTO.
        /// Throws <see cref="ArgumentException"/> if required fields are missing.
        /// Throws <see cref="InvalidOperationException"/> if a record with the same (Year, Project) already exists.
        /// </summary>
        /// <param name="dto">Populated DTO to insert.</param>
        Task<YearlyFinancialDataDto> CreateAsync(YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: Update — updates existing row identified by composite key;
        //   validates record exists before update
        /// <summary>
        /// Updates an existing yearly financial data record and returns the updated DTO.
        /// Throws <see cref="ArgumentException"/> if required fields are missing.
        /// Throws <see cref="KeyNotFoundException"/> if no record exists for the given (Year, Project).
        /// </summary>
        /// <param name="dto">DTO containing updated values; Year and Project identify the row.</param>
        Task<YearlyFinancialDataDto> UpdateAsync(YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: Delete — removes row identified by composite key
        /// <summary>
        /// Deletes the yearly financial data record for the given composite key.
        /// Returns <c>true</c> if the row was found and deleted; <c>false</c> if not found.
        /// </summary>
        /// <param name="year">Financial year (smallint).</param>
        /// <param name="project">Project code (varchar(20)).</param>
        Task<bool> DeleteAsync(short year, string project);

        // TRANSFORMENGINE: PACT actuals query — reads from vpactprojectyearcosts view;
        //   used by "Update Costing" button (btnUpdateCosting) to populate actual spend values
        /// <summary>
        /// Returns aggregated PACT actual cost DTOs from the <c>vpactprojectyearcosts</c> view
        /// for the given project and year.
        /// </summary>
        /// <param name="project">Project code to filter by.</param>
        /// <param name="year">Financial year to filter by.</param>
        Task<IReadOnlyList<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year);
    }
}
