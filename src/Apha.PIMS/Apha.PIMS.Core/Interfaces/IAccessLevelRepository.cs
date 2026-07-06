/*
 * TRANSFORMENGINE MIGRATION — IAccessLevelRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for AccessLevel lookup operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (systemid, accesslevelid) — reflected in method signatures
 *   - GetBySystemIdAsync supports listing all access levels for a given access system
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
    // TRANSFORMENGINE: interface covers lookup/CRUD for AccessLevel (mabarchive.tblaccesslevels); composite PK (systemid, accesslevelid)
    public interface IAccessLevelRepository
    {
        Task<List<AccessLevel>> GetAllAsync();

        Task<List<AccessLevel>> GetBySystemIdAsync(int systemid);

        Task<AccessLevel?> GetByIdAsync(int systemid, int accesslevelid);

        Task<AccessLevel> AddAsync(AccessLevel entity);

        Task<AccessLevel> UpdateAsync(AccessLevel entity);

        Task DeleteAsync(int systemid, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, int accesslevelid);
    }
}
