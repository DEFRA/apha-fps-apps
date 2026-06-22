/*
 * TRANSFORMENGINE MIGRATION — CostCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New frontend service implementation for cost centre maintenance operations
 *   - Injects IFpsApiClient (aggregate) and delegates every method to _fpsClient.FpsCostCentre
 *   - Implements all 6 methods declared on ICostCentreService
 *   - No business logic — pure thin delegate pattern (all logic lives in backend Apha.FPS.Application.Services)
 *   - private readonly _fpsClient — Sonar S2933 compliance
 *
 * PRESERVED:
 *   - All method signatures mirror ICostCentreService exactly
 *   - double key type (costCentreNo) consistent with backend composite key and API client
 *   - CostCentreWorkgroupDto used for workgroup lookup return type
 *   - QueryParameters<string> for paged list endpoint consistent with other FPS services
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register ICostCentreService / CostCentreService in Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs (Phase 10).
 *   - TRANSFORMENGINE TODO: Confirm that UpdateCostCentreAsync double route param is formatted culture-invariant in the HttpClient implementation (Phase 9).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for the Cost Centre maintenance resource.
    /// Thin delegate — all calls forwarded to <see cref="IFpsApiClient.FpsCostCentre"/> with no business logic.
    /// </summary>
    public class CostCentreService : ICostCentreService
    {
        // TRANSFORMENGINE: private readonly _fpsClient — Sonar S2933 compliance; aggregate API client injected via DI
        private readonly IFpsApiClient _fpsClient;

        public CostCentreService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsCostCentre.GetAllCostCentresAsync (workgroup lookup, no logic added)
        public async Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync()
        {
            return await _fpsClient.FpsCostCentre.GetAllCostCentresAsync();
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsCostCentre.GetAllCostCentresPagedAsync (DataGrid paged list)
        public async Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsCostCentre.GetAllCostCentresPagedAsync(query);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsCostCentre.GetCostCentreByIdAsync; FpsYear resolved server-side
        public async Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo)
        {
            return await _fpsClient.FpsCostCentre.GetCostCentreByIdAsync(costCentreNo);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsCostCentre.CreateCostCentreAsync
        public async Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            return await _fpsClient.FpsCostCentre.CreateCostCentreAsync(costCentreDto);
        }

        // TRANSFORMENGINE: thin delegate — costCentreNo forwarded as route identifier for update; supports CostCentreNo changes on existing records
        public async Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto)
        {
            return await _fpsClient.FpsCostCentre.UpdateCostCentreAsync(costCentreNo, costCentreDto);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsCostCentre.DeleteCostCentreAsync
        public async Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo)
        {
            return await _fpsClient.FpsCostCentre.DeleteCostCentreAsync(costCentreNo);
        }
    }
}
