/*
 * TRANSFORMENGINE MIGRATION — IWorkGroupEmployeeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service interface created as thin-delegate contract for WorkGroupEmployee operations
 *   - Mirrors IFpsWorkGroupEmployeeApiClient signatures exactly (dual-DTO design: base + staff variants)
 *   - Namespace: Apha.FPSApps.Application.Interfaces.FPS
 *
 * PRESERVED:
 *   - All method signatures match IFpsWorkGroupEmployeeApiClient (wgGrade filter, pactId key, dual-DTO design)
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify controller only injects IWorkGroupEmployeeService (not IFpsApiClient directly)
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupEmployeeService
    {
        // TRANSFORMENGINE: Base WorkGroupEmployee endpoints — read/update operations
        Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: Staff-variant endpoints — full CRUD on WorkGroupEmployeeStaffDto
        Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);

        // TRANSFORMENGINE: Shared delete — pactId key used by both base and staff variants
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
