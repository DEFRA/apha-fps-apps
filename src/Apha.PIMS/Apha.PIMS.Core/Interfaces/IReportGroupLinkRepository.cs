/*
 * TRANSFORMENGINE MIGRATION — IReportGroupLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ReportGroupLink CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (reportid, groupid) reflected in method signatures
 *   - GetByReportIdAsync supports listing all group links for a given report
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) in this Core interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: interface covers CRUD for ReportGroupLink (mabarchive.tblreportgroup_link); composite PK (reportid, groupid)
    public interface IReportGroupLinkRepository
    {
        Task<List<ReportGroupLink>> GetAllAsync();

        Task<List<ReportGroupLink>> GetByReportIdAsync(int reportid);

        Task<ReportGroupLink?> GetByIdAsync(int reportid, int groupid);

        Task<ReportGroupLink> AddAsync(ReportGroupLink entity);

        Task DeleteAsync(int reportid, int groupid);

        Task<bool> ExistsAsync(int reportid, int groupid);
    }
}
