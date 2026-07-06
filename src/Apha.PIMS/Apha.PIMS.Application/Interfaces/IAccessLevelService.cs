/*
 * TRANSFORMENGINE MIGRATION — IAccessLevelService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for AccessLevel CRUD/lookup operations (Admin Maintenance Tab access level dropdown, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (systemid, accesslevelid) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetBySystemIdAsync supports listing all access levels for a given access system
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
    // TRANSFORMENGINE: service interface for AccessLevel CRUD/lookup; composite PK (systemid, accesslevelid); consumed by AccessLevelController (Phase 5)
    public interface IAccessLevelService
    {
        Task<List<AccessLevelDto>> GetAllAsync();

        // TRANSFORMENGINE: returns all access levels for a given system — used for dropdown population
        Task<List<AccessLevelDto>> GetBySystemIdAsync(int systemid);

        Task<AccessLevelDto?> GetByIdAsync(int systemid, int accesslevelid);

        Task<AccessLevelDto> CreateAsync(AccessLevelDto dto);

        Task<AccessLevelDto> UpdateAsync(AccessLevelDto dto);

        Task DeleteAsync(int systemid, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, int accesslevelid);
    }
}
