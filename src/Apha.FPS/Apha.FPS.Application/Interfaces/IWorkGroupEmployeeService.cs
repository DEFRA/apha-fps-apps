// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — IWorkGroupEmployeeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto) method signature
 *     to support the Add/Create operation inferred from fps_maintain_wg_staff.js (rowId == null branch)
 *
 * PRESERVED:
 *   - GetWorkGroupEmployeeAsync, GetWorkGroupEmployeeByIdAsync, UpdateWorkGroupEmployeeAsync,
 *     DeleteWorkGroupEmployeeAsync signatures unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm HrsAvail computation responsibility — service layer vs repository.
 *     Current plan defers HrsAvail = HrsPaid - Leave - SickSpecial to the DataAccess layer (Phase 4).
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupEmployeeService
    {
        Task<PaginatedResult<WorkGroupEmployeeDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<WorkGroupEmployeeDto?> GetWorkGroupEmployeeByIdAsync(string pactId);

        // TRANSFORMENGINE: CreateWorkGroupEmployeeAsync added — corresponds to POST /api/v1/wgstaff
        // inferred from fps_maintain_wg_staff.js Add button / rowId == null branch
        Task<WorkGroupEmployeeDto> CreateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);

        Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);
        Task<bool> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
