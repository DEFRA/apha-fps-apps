/*
 * TRANSFORMENGINE MIGRATION — IReportGroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ReportGroup lookup/CRUD operations (Reports Tab, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - GetAllAsync returns full list for report-group dropdown / lookup usage
 *   - GetByIdAsync returns nullable to signal not-found without throwing
 *   - Full CRUD surface retained to match IReportGroupRepository capabilities
 *
 * PRESERVED:
 *   - No infrastructure-specific code in this Application interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    // TRANSFORMENGINE: service interface for ReportGroup lookup/CRUD; consumed by ReportGroupController (Phase 5); backed by IReportGroupRepository
    public interface IReportGroupService
    {
        Task<List<ReportGroupDto>> GetAllAsync();

        Task<ReportGroupDto?> GetByIdAsync(int groupid);

        Task<ReportGroupDto> CreateAsync(ReportGroupDto dto);

        Task<ReportGroupDto> UpdateAsync(ReportGroupDto dto);

        Task DeleteAsync(int groupid);

        Task<bool> ExistsAsync(int groupid);
    }
}
