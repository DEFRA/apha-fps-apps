/*
 * TRANSFORMENGINE MIGRATION — IReportService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for Report CRUD operations (Reports Tab, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - GetAllAsync returns full list for grid display
 *   - GetByIdAsync returns nullable to signal not-found without throwing
 *   - CreateAsync / UpdateAsync / DeleteAsync preserve the full CRUD surface
 *   - ExistsAsync supports duplicate-name guards in controller layer
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
    // TRANSFORMENGINE: service interface for Report CRUD; consumed by ReportController (Phase 5); backed by IReportRepository
    public interface IReportService
    {
        Task<List<ReportDto>> GetAllAsync();

        Task<ReportDto?> GetByIdAsync(int id);

        Task<ReportDto> CreateAsync(ReportDto dto);

        Task<ReportDto> UpdateAsync(ReportDto dto);

        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
