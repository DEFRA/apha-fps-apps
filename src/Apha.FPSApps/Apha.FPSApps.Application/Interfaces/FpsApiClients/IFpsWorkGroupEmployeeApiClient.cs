// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IFpsWorkGroupEmployeeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto) method signature
 *     mirrors the POST /api/v1/wgstaff backend action added to WorkGroupEmployeeController in Phase 6
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync — matches GET /api/v1/wgstaff?wgGrade={wgGrade} + pagination
 *   - GetWorkGroupEmployeeByIdAsync — matches GET /api/v1/wgstaff/{pactId}
 *   - UpdateWorkGroupEmployeeAsync — matches PUT /api/v1/wgstaff
 *   - DeleteWorkGroupEmployeeAsync — matches DELETE /api/v1/wgstaff/{pactId}
 *   - All return types wrapped in ApiResponseDto<T>
 *   - Namespace Apha.FPSApps.Application.Interfaces.FpsApiClients unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: wgGrade parameter in GetWorkGroupEmployeeAsync must be sourced from
 *     parent page context, URL route, or session state — confirm the MaintWGStaff page supplies it.
 *   - TRANSFORMENGINE TODO: Verify FpsWorkGroupEmployeeApiClient implementation adds the
 *     CreateWorkGroupEmployeeAsync HTTP POST call to api/v1/wgstaff (Phase 9 infrastructure file).
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWorkGroupEmployeeApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/wgstaff — paginated list filtered by wgGrade
        Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);

        // TRANSFORMENGINE: GET /api/v1/wgstaff/{pactId}
        Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);

        // TRANSFORMENGINE: POST /api/v1/wgstaff — Create new WorkGroupEmployee (added Phase 7, matches backend Phase 6 POST action)
        Task<ApiResponseDto<WorkGroupEmployeeDto>> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: PUT /api/v1/wgstaff — Update existing WorkGroupEmployee
        Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/wgstaff/{pactId}
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
