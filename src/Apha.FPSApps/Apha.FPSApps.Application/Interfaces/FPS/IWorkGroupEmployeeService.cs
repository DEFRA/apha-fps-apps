// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IWorkGroupEmployeeService.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto) method signature
 *     mirrors IFpsWorkGroupEmployeeApiClient.CreateWorkGroupEmployeeAsync added in Phase 7
 *     and the POST /api/v1/wgstaff backend action added to WorkGroupEmployeeController in Phase 6
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync — paginated list filtered by wgGrade
 *   - GetWorkGroupEmployeeByIdAsync — get single record by pactId
 *   - UpdateWorkGroupEmployeeAsync — update existing record
 *   - DeleteWorkGroupEmployeeAsync — delete by pactId
 *   - All return types wrapped in ApiResponseDto<T>
 *   - Namespace Apha.FPSApps.Application.Interfaces.FPS unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm wgGrade parameter source in MaintWGStaff page (URL route, session, or parent page context)
 *     before Phase 9 MVC controller is wired up.
 *   - TRANSFORMENGINE TODO: Verify IFpsWorkGroupEmployeeApiClient implementation (FpsWorkGroupEmployeeApiClient.cs)
 *     has the CreateWorkGroupEmployeeAsync HTTP POST call to api/v1/wgstaff before invoking this service.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupEmployeeService
    {
        // TRANSFORMENGINE: GET /api/v1/wgstaff — paginated list filtered by wgGrade
        Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);

        // TRANSFORMENGINE: GET /api/v1/wgstaff/{pactId}
        Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);

        // TRANSFORMENGINE: POST /api/v1/wgstaff — Create new WorkGroupEmployee (added Phase 8, matches backend Phase 6 POST action)
        Task<ApiResponseDto<WorkGroupEmployeeDto>> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: PUT /api/v1/wgstaff — Update existing WorkGroupEmployee
        Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/wgstaff/{pactId}
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
