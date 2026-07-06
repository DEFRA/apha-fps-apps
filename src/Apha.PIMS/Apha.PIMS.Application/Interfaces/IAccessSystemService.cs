/*
 * TRANSFORMENGINE MIGRATION — IAccessSystemService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for AccessSystem lookup operations (system filter dropdown, frmMaintainance / admin.js)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (systemid) reflected in GetByIdAsync / ExistsAsync
 *   - GetAllAsync returns full list for system dropdown / lookup usage
 *   - No CreateAsync / UpdateAsync / DeleteAsync — systems are reference data, not user-managed records
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
    // TRANSFORMENGINE: service interface for AccessSystem lookup; single integer PK (systemid); read-only reference data; consumed by AccessSystemController (Phase 5)
    public interface IAccessSystemService
    {
        Task<List<AccessSystemDto>> GetAllAsync();

        Task<AccessSystemDto?> GetByIdAsync(int systemid);

        Task<bool> ExistsAsync(int systemid);
    }
}
