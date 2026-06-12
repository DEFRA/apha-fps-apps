// TRANSFORMENGINE: human_review — verify before running
/*
 * TRANSFORMENGINE MIGRATION — IWorkGroupEmployeeRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added CreateWorkGroupEmployeeAsync(WorkGroupEmployee entity) method signature
 *     Required by the Add Staff functionality inferred from fps_maintain_wg_staff.js (rowId == null branch → POST /api/v1/wgstaff)
 *
 * PRESERVED:
 *   - All 5 existing method signatures: GetWorkGroupEmployeeAsync, GetWorkGroupEmployeeByIdAsync,
 *     UpdateWorkGroupEmployeeAsync, DeleteWorkGroupEmployeeAsync, HasAssociatedStaffAsync
 *   - Pagination contract: PagedData<WorkGroupEmployeeView> with PaginationParameters<string>
 *   - No infrastructure-specific code (no DbContext, no EF references) in Core layer
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify wgGrade parameter on GetWorkGroupEmployeeAsync — confirm it maps to the user-context-filtered query in repository
 *   - TRANSFORMENGINE TODO: Confirm GetWorkGroupEmployeeByIdAsync(string pactId) is sufficient — composite PK is (pactid, fpsyear); may need overload with fpsYear if cross-year lookup is required
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupEmployeeRepository
    {
        // TRANSFORMENGINE: paginated list filtered by wgGrade — maps to GET /api/v1/wgstaff?wgGrade=&page=&pageSize=
        Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(PaginationParameters<string> query, string wgGrade);

        // TRANSFORMENGINE: single record lookup — maps to GET /api/v1/wgstaff/{pactId}
        Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId);

        // TRANSFORMENGINE: create new record — maps to POST /api/v1/wgstaff; HrsAvail must be computed before persist (HrsPaid - Leave - SickSpecial)
        Task<WorkGroupEmployee> CreateWorkGroupEmployeeAsync(WorkGroupEmployee entity);

        // TRANSFORMENGINE: update existing record — maps to PUT /api/v1/wgstaff
        Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity);

        // TRANSFORMENGINE: soft/hard delete by pactId — maps to DELETE /api/v1/wgstaff/{pactId}
        Task<bool> DeleteWorkGroupEmployeeAsync(string pactId);

        // TRANSFORMENGINE: existence check for cascade/guard logic — used before allowing WG Grade deletion
        Task<bool> HasAssociatedStaffAsync(string wgGrade);
    }
}
