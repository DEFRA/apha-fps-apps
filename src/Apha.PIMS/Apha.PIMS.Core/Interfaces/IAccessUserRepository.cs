/*
 * TRANSFORMENGINE MIGRATION — IAccessUserRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for AccessUser CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (systemid, ntlogin) — reflected in method signatures
 *   - GetBySystemIdAsync supports listing all users for a given access system
 *   - GetByNtLoginAsync supports lookup of a user by NT login across all systems
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
    // TRANSFORMENGINE: interface covers CRUD for AccessUser (mabarchive.tblaccessusers); composite PK (systemid, ntlogin)
    public interface IAccessUserRepository
    {
        Task<List<AccessUser>> GetAllAsync();

        Task<List<AccessUser>> GetBySystemIdAsync(int systemid);

        Task<List<AccessUser>> GetByNtLoginAsync(string ntlogin);

        Task<AccessUser?> GetByIdAsync(int systemid, string ntlogin);

        Task<AccessUser> AddAsync(AccessUser entity);

        Task<AccessUser> UpdateAsync(AccessUser entity);

        Task DeleteAsync(int systemid, string ntlogin);

        Task<bool> ExistsAsync(int systemid, string ntlogin);
    }
}
