/*
 * TRANSFORMENGINE MIGRATION — IReportGroupRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ReportGroup lookup operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *   - GetAllAsync returns full list for group dropdown / lookup usage
 *   - GetByIdAsync returns nullable to signal not-found without throwing
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
    // TRANSFORMENGINE: interface covers lookup operations for ReportGroup (mabarchive.tblreportgroup); used by ReportGroupService (Phase 3)
    public interface IReportGroupRepository
    {
        Task<List<ReportGroup>> GetAllAsync();

        Task<ReportGroup?> GetByIdAsync(int groupid);

        Task<ReportGroup> AddAsync(ReportGroup entity);

        Task<ReportGroup> UpdateAsync(ReportGroup entity);

        Task DeleteAsync(int groupid);

        Task<bool> ExistsAsync(int groupid);
    }
}
