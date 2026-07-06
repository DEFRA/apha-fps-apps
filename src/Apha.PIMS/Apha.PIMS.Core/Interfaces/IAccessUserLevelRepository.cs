/*
 * TRANSFORMENGINE MIGRATION — IAccessUserLevelRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for AccessUserLevel CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Three-column composite PK (systemid, ntlogin, accesslevelid) — reflected in method signatures
 *   - GetByUserAsync supports listing all level assignments for a given user (systemid + ntlogin)
 *   - GetBySystemIdAsync supports listing all user-level assignments for an access system
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
    // TRANSFORMENGINE: interface covers CRUD for AccessUserLevel (mabarchive.tblaccessusers_levels); three-column composite PK (systemid, ntlogin, accesslevelid)
    public interface IAccessUserLevelRepository
    {
        Task<List<AccessUserLevel>> GetAllAsync();

        Task<List<AccessUserLevel>> GetBySystemIdAsync(int systemid);

        Task<List<AccessUserLevel>> GetByUserAsync(int systemid, string ntlogin);

        Task<AccessUserLevel?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid);

        Task<AccessUserLevel> AddAsync(AccessUserLevel entity);

        Task DeleteAsync(int systemid, string ntlogin, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid);
    }
}
