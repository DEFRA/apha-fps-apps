/*
 * TRANSFORMENGINE MIGRATION — IPimsYearlyFinancialDataApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: typed API client interface for the YearlyFinancialData backend resource
 *   - Method signatures match backend YearlyFinancialDataController routes confirmed in Phase 5:
 *       GET  api/v1/yearlyfinancialdata/{project}          → GetAllAsync
 *       GET  api/v1/yearlyfinancialdata/{year:int}/{project} → GetByKeyAsync
 *       POST api/v1/yearlyfinancialdata                    → CreateAsync
 *       PUT  api/v1/yearlyfinancialdata/{year:int}/{project} → UpdateAsync
 *       DELETE api/v1/yearlyfinancialdata/{year:int}/{project} → DeleteAsync
 *       GET  api/v1/yearlyfinancialdata/{project}/{year:int}/pactcosts → GetPactCostsAsync
 *   - Composite key parameters (year: short, project: string) required on GetByKeyAsync,
 *     UpdateAsync, DeleteAsync — both are sourced from the grid row selected by the user
 *   - GetPactCostsAsync returns PactProjectYearCostsDto (mirrors PactProjectYearCostsRes);
 *     used by "Update Costing" button flow to pre-populate modal cost fields
 *
 * PRESERVED:
 *   - Return types wrapped in ApiResponseDto<T> per project envelope convention
 *   - QueryParameters<string> used for the paginated list endpoint
 *   - project filter parameter passed as route segment to GetAllAsync (required by backend)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether backend GetAllAsync requires project as a mandatory
 *     route param (current: required string) or may be optional — verify against
 *     YearlyFinancialDataController.GetAll action signature
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    /// <summary>
    /// Typed HTTP API client interface for the YearlyFinancialData backend resource.
    /// Maps to routes under <c>api/v1/yearlyfinancialdata</c> on the PIMS backend.
    /// Composite key: (<see cref="short"/> year, <see cref="string"/> project).
    /// </summary>
    public interface IPimsYearlyFinancialDataApiClient
    {
        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{project} — paginated list filtered by project
        /// <summary>Gets all yearly financial data records for a given project, with pagination/sorting/search.</summary>
        Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(string project, QueryParameters<string> query);

        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{year:int}/{project} — single record by composite key
        /// <summary>Gets a single yearly financial data record by composite key (year + project).</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project);

        // TRANSFORMENGINE: POST api/v1/yearlyfinancialdata — create new record (Year + Project in body)
        /// <summary>Creates a new yearly financial data record. Year and Project must be set on the DTO.</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: PUT api/v1/yearlyfinancialdata/{year:int}/{project} — update existing record
        /// <summary>Updates an existing yearly financial data record identified by composite key (year + project).</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(short year, string project, YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: DELETE api/v1/yearlyfinancialdata/{year:int}/{project} — delete by composite key
        /// <summary>Deletes a yearly financial data record by composite key (year + project).</summary>
        Task<ApiResponseDto<bool>> DeleteAsync(short year, string project);

        // TRANSFORMENGINE: GET api/v1/yearlyfinancialdata/{project}/{year:int}/pactcosts — "Update Costing" button
        /// <summary>
        /// Gets PACT actuals aggregation for the given project and year.
        /// Used by the "Update Costing" button to pre-populate modal cost fields before saving.
        /// </summary>
        Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year);
    }
}
