/*
 * TRANSFORMENGINE MIGRATION — IYearlyFinancialDataService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: frontend service interface for the YearlyFinancialData resource family
 *   - Signatures mirror IPimsYearlyFinancialDataApiClient exactly
 *   - Composite key parameters (short year, string project) match backend controller routes:
 *       GET  api/v1/yearlyfinancialdata/{project}              → GetAllAsync
 *       GET  api/v1/yearlyfinancialdata/{year:int}/{project}   → GetByKeyAsync
 *       POST api/v1/yearlyfinancialdata                        → CreateAsync
 *       PUT  api/v1/yearlyfinancialdata/{year:int}/{project}   → UpdateAsync
 *       DELETE api/v1/yearlyfinancialdata/{year:int}/{project} → DeleteAsync
 *       GET  api/v1/yearlyfinancialdata/{project}/{year:int}/pactcosts → GetPactCostsAsync
 *   - GetPactCostsAsync included for "Update Costing" button modal pre-population flow
 *
 * PRESERVED:
 *   - Return types wrapped in ApiResponseDto<T> per project envelope convention
 *   - QueryParameters<string> used for the paginated list endpoint
 *   - Composite key parameter order (year first, project second) matches API client
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify GetAllAsync project param is always required (non-nullable string)
 *     once frontend page context is confirmed — backend route currently requires project as a route segment
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    /// <summary>
    /// Frontend service interface for the YearlyFinancialData resource.
    /// Thin delegate — all methods forward to <see cref="Interfaces.PimsApiClients.IPimsYearlyFinancialDataApiClient"/>.
    /// Composite key: (<see cref="short"/> year, <see cref="string"/> project).
    /// </summary>
    public interface IYearlyFinancialDataService
    {
        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.GetAllAsync
        //                  → GET api/v1/yearlyfinancialdata/{project} (paginated, filtered by project)
        /// <summary>Gets all yearly financial data records for a given project, with pagination/sorting/search.</summary>
        Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(string project, QueryParameters<string> query);

        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.GetByKeyAsync
        //                  → GET api/v1/yearlyfinancialdata/{year:int}/{project}
        /// <summary>Gets a single yearly financial data record by composite key (year + project).</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project);

        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.CreateAsync
        //                  → POST api/v1/yearlyfinancialdata (Year + Project set on DTO body)
        /// <summary>Creates a new yearly financial data record. Year and Project must be set on the DTO.</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.UpdateAsync
        //                  → PUT api/v1/yearlyfinancialdata/{year:int}/{project}
        /// <summary>Updates an existing yearly financial data record identified by composite key (year + project).</summary>
        Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(short year, string project, YearlyFinancialDataDto dto);

        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.DeleteAsync
        //                  → DELETE api/v1/yearlyfinancialdata/{year:int}/{project}
        /// <summary>Deletes a yearly financial data record by composite key (year + project).</summary>
        Task<ApiResponseDto<bool>> DeleteAsync(short year, string project);

        // TRANSFORMENGINE: mirrors IPimsYearlyFinancialDataApiClient.GetPactCostsAsync
        //                  → GET api/v1/yearlyfinancialdata/{project}/{year:int}/pactcosts
        //                  Used by "Update Costing" button to pre-populate modal cost fields
        /// <summary>
        /// Gets PACT actuals aggregation for the given project and year.
        /// Used by the "Update Costing" button to pre-populate modal cost fields before saving.
        /// </summary>
        Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year);
    }
}
