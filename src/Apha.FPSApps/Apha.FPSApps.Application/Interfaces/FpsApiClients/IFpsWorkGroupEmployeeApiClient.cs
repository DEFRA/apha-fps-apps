/*
 * TRANSFORMENGINE MIGRATION — IFpsWorkGroupEmployeeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend API client interface created defining HTTP contract for WorkGroupEmployee backend endpoints
 *   - Namespace scoped to Apha.FPSApps.Application.Interfaces.FpsApiClients
 *   - Dual-endpoint design: base (read/update) vs staff (full CRUD + create) to match two distinct
 *     backend controller action groups
 *
 * PRESERVED:
 *   - All method signatures exactly match backend API controller endpoint signatures
 *   - wgGrade filter parameter on list endpoints preserves backend filtering contract
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify backend route paths match FpsWorkGroupEmployeeApiClient implementation
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWorkGroupEmployeeApiClient
    {
        // TRANSFORMENGINE: Base WorkGroupEmployee endpoints — read/update operations (no create/delete on base DTO)
        Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        // TRANSFORMENGINE: Staff-variant endpoints — full CRUD on WorkGroupEmployeeStaffDto (extended fields)
        Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);

        // TRANSFORMENGINE: Shared delete endpoint — pactId key used by both base and staff variants
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
