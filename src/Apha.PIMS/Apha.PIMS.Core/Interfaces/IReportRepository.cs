/*
 * TRANSFORMENGINE MIGRATION — IReportRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for Report CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *   - GetAllAsync returns full list for grid display (Reports Tab in frmMaintainance)
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
    // TRANSFORMENGINE: interface covers full CRUD required by ReportService (Phase 3) and ReportController (Phase 5)
    public interface IReportRepository
    {
        Task<List<Report>> GetAllAsync();

        Task<Report?> GetByIdAsync(int id);

        Task<Report> AddAsync(Report entity);

        Task<Report> UpdateAsync(Report entity);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
