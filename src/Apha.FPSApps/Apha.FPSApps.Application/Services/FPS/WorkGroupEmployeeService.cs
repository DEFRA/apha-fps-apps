/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service implementation created as thin delegate forwarding to IFpsApiClient.FpsWorkGroupEmployee
 *   - Implements IWorkGroupEmployeeService; injects IFpsApiClient (Sonar S2933: private readonly enforced)
 *   - All 8 method bodies are single-line return await delegates — no business logic
 *
 * PRESERVED:
 *   - All method signatures match IWorkGroupEmployeeService / IFpsWorkGroupEmployeeApiClient exactly
 *   - Dual-DTO design: base (WorkGroupEmployeeDto) vs staff (WorkGroupEmployeeStaffDto) variants preserved
 *   - wgGrade filter parameter and pactId key type preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated thin-delegate pattern
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
        // TRANSFORMENGINE: S2933 — private readonly field, injected via constructor
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupEmployeeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: Thin delegate — forwards to _fpsClient.FpsWorkGroupEmployee (no logic)
        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdAsync(pactId);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeAsync(dto);
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeForStaffAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdForStaffAsync(pactId);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.CreateWorkGroupEmployeeForStaffAsync(dto);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeForStaffAsync(dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}
