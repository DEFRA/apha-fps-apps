// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeService.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto) thin-delegate method
 *     forwarding to _fpsClient.FpsWorkGroupEmployee.CreateWorkGroupEmployeeAsync(dto)
 *     mirrors the POST /api/v1/wgstaff backend action (Phase 6) and API client interface (Phase 7)
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync — delegates to _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeAsync
 *   - GetWorkGroupEmployeeByIdAsync — delegates to _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdAsync
 *   - UpdateWorkGroupEmployeeAsync — delegates to _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeAsync
 *   - DeleteWorkGroupEmployeeAsync — delegates to _fpsClient.FpsWorkGroupEmployee.DeleteWorkGroupEmployeeAsync
 *   - Constructor injection of IFpsApiClient as _fpsClient (private readonly — Sonar S2933 compliant)
 *   - Namespace Apha.FPSApps.Application.Services.FPS unchanged
 *   - Thin delegate pattern: no business logic, no conditionals — Sonar S4144 compliant
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify FpsWorkGroupEmployeeApiClient.CreateWorkGroupEmployeeAsync
 *     is fully implemented in the infrastructure layer (HTTP POST to api/v1/wgstaff) before
 *     wiring the MVC controller in Phase 9.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupEmployeeService : IWorkGroupEmployeeService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliant
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupEmployeeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: thin delegate — GET /api/v1/wgstaff?wgGrade={wgGrade}
        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeAsync(query, wgGrade);
        }

        // TRANSFORMENGINE: thin delegate — GET /api/v1/wgstaff/{pactId}
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdAsync(pactId);
        }

        // TRANSFORMENGINE: thin delegate — POST /api/v1/wgstaff (added Phase 8)
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.CreateWorkGroupEmployeeAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate — PUT /api/v1/wgstaff
        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate — DELETE /api/v1/wgstaff/{pactId}
        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}
