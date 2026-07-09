/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: thin frontend service implementation for the YearlyFinancialData resource family
 *   - Injects IPimsApiClient; all methods delegate to _client.PimsYearlyFinancialData
 *   - No business logic — single-expression method bodies (thin delegate pattern)
 *   - Composite key parameters (short year, string project) forwarded verbatim to API client
 *
 * PRESERVED:
 *   - Method signatures identical to IYearlyFinancialDataService (Phase 8 interface)
 *   - Return types wrapped in ApiResponseDto<T> per project envelope convention
 *   - GetPactCostsAsync parameter order matches IPimsYearlyFinancialDataApiClient (project first, year second)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register IYearlyFinancialDataService → YearlyFinancialDataService in
 *     Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs (Phase 9 scope)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    /// <summary>
    /// Frontend service implementation for the YearlyFinancialData resource.
    /// Thin delegate — every method forwards to <see cref="IPimsApiClient.PimsYearlyFinancialData"/>.
    /// No business logic resides here; all validation and transformation live in the backend service.
    /// </summary>
    public class YearlyFinancialDataService : IYearlyFinancialDataService
    {
        // TRANSFORMENGINE: S2933 — field is private readonly per Sonar rule
        private readonly IPimsApiClient _client;

        public YearlyFinancialDataService(IPimsApiClient client)
        {
            _client = client;
        }

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.GetAllAsync
        //                  GET api/v1/yearlyfinancialdata/{project}
        public async Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(string project, QueryParameters<string> query)
            => await _client.PimsYearlyFinancialData.GetAllAsync(project, query);

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.GetByKeyAsync
        //                  GET api/v1/yearlyfinancialdata/{year:int}/{project}
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project)
            => await _client.PimsYearlyFinancialData.GetByKeyAsync(year, project);

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.CreateAsync
        //                  POST api/v1/yearlyfinancialdata
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto)
            => await _client.PimsYearlyFinancialData.CreateAsync(dto);

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.UpdateAsync
        //                  PUT api/v1/yearlyfinancialdata/{year:int}/{project}
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(short year, string project, YearlyFinancialDataDto dto)
            => await _client.PimsYearlyFinancialData.UpdateAsync(year, project, dto);

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.DeleteAsync
        //                  DELETE api/v1/yearlyfinancialdata/{year:int}/{project}
        public async Task<ApiResponseDto<bool>> DeleteAsync(short year, string project)
            => await _client.PimsYearlyFinancialData.DeleteAsync(year, project);

        // TRANSFORMENGINE: thin delegate → _client.PimsYearlyFinancialData.GetPactCostsAsync
        //                  GET api/v1/yearlyfinancialdata/{project}/{year:int}/pactcosts
        //                  "Update Costing" button pre-populate modal cost fields
        public async Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
            => await _client.PimsYearlyFinancialData.GetPactCostsAsync(project, year);
    }
}
