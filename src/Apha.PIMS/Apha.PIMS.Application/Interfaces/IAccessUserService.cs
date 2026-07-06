/*
 * TRANSFORMENGINE MIGRATION — IAccessUserService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for AccessUser CRUD operations (Admin Maintenance Tab users grid, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (systemid, ntlogin) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetBySystemIdAsync supports listing all users for a given access system
 *   - GetByNtLoginAsync supports cross-system lookup of a user by NT login
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
    // TRANSFORMENGINE: service interface for AccessUser CRUD; composite PK (systemid, ntlogin); consumed by AccessUserController (Phase 5)
    public interface IAccessUserService
    {
        Task<List<AccessUserDto>> GetAllAsync();

        Task<List<AccessUserDto>> GetBySystemIdAsync(int systemid);

        // TRANSFORMENGINE: cross-system lookup by NT login — used for user search/autocomplete
        Task<List<AccessUserDto>> GetByNtLoginAsync(string ntlogin);

        Task<AccessUserDto?> GetByIdAsync(int systemid, string ntlogin);

        Task<AccessUserDto> CreateAsync(AccessUserDto dto);

        Task<AccessUserDto> UpdateAsync(AccessUserDto dto);

        Task DeleteAsync(int systemid, string ntlogin);

        Task<bool> ExistsAsync(int systemid, string ntlogin);
    }
}
