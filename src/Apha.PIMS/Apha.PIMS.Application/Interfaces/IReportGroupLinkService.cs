/*
 * TRANSFORMENGINE MIGRATION — IReportGroupLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ReportGroupLink CRUD operations (Reports Tab sub-grid, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (reportid, groupid) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetByReportIdAsync supports listing all group-links for a given report
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
    // TRANSFORMENGINE: service interface for ReportGroupLink CRUD; composite PK (reportid, groupid); consumed by ReportGroupLinkController (Phase 5)
    public interface IReportGroupLinkService
    {
        Task<List<ReportGroupLinkDto>> GetAllAsync();

        Task<List<ReportGroupLinkDto>> GetByReportIdAsync(int reportid);

        Task<ReportGroupLinkDto?> GetByIdAsync(int reportid, int groupid);

        Task<ReportGroupLinkDto> CreateAsync(ReportGroupLinkDto dto);

        Task DeleteAsync(int reportid, int groupid);

        Task<bool> ExistsAsync(int reportid, int groupid);
    }
}
