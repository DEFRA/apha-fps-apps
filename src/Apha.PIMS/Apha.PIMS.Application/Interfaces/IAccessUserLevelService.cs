/*
 * TRANSFORMENGINE MIGRATION — IAccessUserLevelService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for AccessUserLevel CRUD operations (Admin Maintenance Tab user-access grid, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Three-column composite PK (systemid, ntlogin, accesslevelid) reflected in method signatures
 *   - GetByUserAsync supports listing all level assignments for a given user (systemid + ntlogin)
 *   - GetBySystemIdAsync supports listing all user-level assignments for an access system
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
    // TRANSFORMENGINE: service interface for AccessUserLevel CRUD; three-column composite PK (systemid, ntlogin, accesslevelid); consumed by AccessUserLevelController (Phase 5)
    public interface IAccessUserLevelService
    {
        Task<List<AccessUserLevelDto>> GetAllAsync();

        Task<List<AccessUserLevelDto>> GetBySystemIdAsync(int systemid);

        // TRANSFORMENGINE: returns all level assignments for a given user — used for user access management grid
        Task<List<AccessUserLevelDto>> GetByUserAsync(int systemid, string ntlogin);

        Task<AccessUserLevelDto?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid);

        Task<AccessUserLevelDto> CreateAsync(AccessUserLevelDto dto);

        Task DeleteAsync(int systemid, string ntlogin, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid);
    }
}
