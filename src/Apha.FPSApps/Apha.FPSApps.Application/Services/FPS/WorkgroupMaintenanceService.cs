/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: Thin frontend service delegate for WorkGroup Maintenance operations
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup)
 *   - Implements IWorkgroupMaintenanceService; all 8 methods delegate to IFpsApiClient.FpsWorkgroupMaintenance
 *   - Constructor injects IFpsApiClient (aggregate API client) with null-guard
 *   - NO business logic — every method body is a single return await delegation
 *   - Dependency graph: MVC Controller → IWorkgroupMaintenanceService → WorkgroupMaintenanceService
 *       → IFpsApiClient.FpsWorkgroupMaintenance → HTTP → backend api/v1/workgroup
 *
 * PRESERVED:
 *   - Method naming mirrors IWorkgroupMaintenanceService exactly
 *   - All return types match interface signatures (ApiResponseDto<T>)
 *   - _client field is private readonly (S2933 compliance)
 *   - Thin-delegate pattern: no if/switch/throw/foreach/for in service methods (S4144 safe)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync delegates List<double?> — if labelled projection
 *     needed, update IFpsWorkgroupApiClient and IWorkgroupMaintenanceService together
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Thin frontend service delegate for WorkGroup Maintenance CRUD and lookup operations.
    /// All method bodies delegate to <see cref="IFpsApiClient.FpsWorkgroupMaintenance"/> without
    /// adding business logic.  Migrated from <c>frmMaintWorkGroup2</c>.
    /// </summary>
    public class WorkgroupMaintenanceService : IWorkgroupMaintenanceService
    {
        // TRANSFORMENGINE: _client is private readonly — S2933 compliance
        private readonly IFpsApiClient _client;

        public WorkgroupMaintenanceService(IFpsApiClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        // ── CRUD ────────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetPagedAsync
        public async Task<ApiResponseDto<List<WorkgroupMaintenanceDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            return await _client.FpsWorkgroupMaintenance.GetPagedAsync(query);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetByWorkGroupNameAsync
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> GetByWorkGroupNameAsync(string workGroupName)
        {
            return await _client.FpsWorkgroupMaintenance.GetByWorkGroupNameAsync(workGroupName);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.CreateAsync
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> CreateAsync(WorkgroupMaintenanceDto dto)
        {
            return await _client.FpsWorkgroupMaintenance.CreateAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.UpdateAsync
        //   workGroupName is the original key (before any rename); dto.WorkGroupName may differ
        public async Task<ApiResponseDto<WorkgroupMaintenanceDto>> UpdateAsync(string workGroupName, WorkgroupMaintenanceDto dto)
        {
            return await _client.FpsWorkgroupMaintenance.UpdateAsync(workGroupName, dto);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.DeleteAsync
        public async Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName)
        {
            return await _client.FpsWorkgroupMaintenance.DeleteAsync(workGroupName);
        }

        // ── Lookup endpoints (SEPARATE from CRUD resource family) ────────────────

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetProfitCentresAsync
        public async Task<ApiResponseDto<List<string>>> GetProfitCentresAsync()
        {
            return await _client.FpsWorkgroupMaintenance.GetProfitCentresAsync();
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetOwnersAsync
        public async Task<ApiResponseDto<List<ManagerDto>>> GetOwnersAsync()
        {
            return await _client.FpsWorkgroupMaintenance.GetOwnersAsync();
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetCostCentresAsync
        //   profitCentre sourced from modal ProfitCentre change event (confirmed page-sourced)
        public async Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre)
        {
            return await _client.FpsWorkgroupMaintenance.GetCostCentresAsync(profitCentre);
        }
    }
}
