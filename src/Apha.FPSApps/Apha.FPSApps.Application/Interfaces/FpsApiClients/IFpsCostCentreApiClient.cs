/*
 * TRANSFORMENGINE MIGRATION — IFpsCostCentreApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New frontend API client interface for cost centre maintenance operations
 *   - Mirrors backend CostCentreController endpoints at route api/v{version}/costcentre
 *   - Exposes 6 async methods matching backend controller actions exactly
 *   - Workgroup lookup (GetAllCostCentresAsync) preserved from original GET / endpoint
 *   - Paged DataGrid (GetAllCostCentresPagedAsync) maps to GET /paged
 *   - CRUD operations (GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync) map to GET/POST/PUT/DELETE
 *
 * PRESERVED:
 *   - CostCentreWorkgroupDto reused for workgroup lookup (already exists, mirrors CostCentreWorkgroupRes)
 *   - Composite key pattern: double costCentreNo mirrors backend route param type
 *   - QueryParameters<string> for paged list endpoint consistent with other FPS API clients
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsYear is supplied server-side via X-FPS-Year header (IFpsRequestContext) — frontend must set this header on all HTTP requests via IFpsHttpExecutor; no FpsYear parameter needed on client methods.
 *   - TRANSFORMENGINE TODO: UpdateAsync passes originalCostCentreNo as route param — confirm the HttpClient implementation passes this correctly (double route param may require culture-invariant string formatting).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsCostCentreApiClient
    {
        // TRANSFORMENGINE: workgroup lookup — mirrors GET api/v1/costcentre (GetAllCostCentresAsync, stored-proc backed)
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync();

        // TRANSFORMENGINE: paged DataGrid — mirrors GET api/v1/costcentre/paged (GetAllCostCentresPagedAsync)
        Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: single-record lookup — mirrors GET api/v1/costcentre/{costCentreNo} (GetCostCentreByIdAsync); FpsYear resolved server-side from request context
        Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo);

        // TRANSFORMENGINE: create — mirrors POST api/v1/costcentre (CreateCostCentreAsync); maps to saveTblCostCentre() in costcenter_maintenance.js
        Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto);

        // TRANSFORMENGINE: update — mirrors PUT api/v1/costcentre/{costCentreNo} (UpdateCostCentreAsync); originalCostCentreNo identifies existing record; maps to updateTblCostCentre() in costcenter_maintenance.js
        Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto);

        // TRANSFORMENGINE: delete — mirrors DELETE api/v1/costcentre/{costCentreNo} (DeleteCostCentreAsync); maps to handleTblCostCentreDelete() in costcenter_maintenance.js
        Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo);
    }
}
