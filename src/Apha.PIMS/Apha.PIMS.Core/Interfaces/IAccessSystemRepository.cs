/*
 * TRANSFORMENGINE MIGRATION — IAccessSystemRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for AccessSystem lookup operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (systemid) — reflected in method signatures
 *   - GetAllAsync returns full list for system dropdown / lookup usage
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
    // TRANSFORMENGINE: interface covers lookup for AccessSystem (mabarchive.tblaccesssystems); single integer PK (systemid)
    public interface IAccessSystemRepository
    {
        Task<List<AccessSystem>> GetAllAsync();

        Task<AccessSystem?> GetByIdAsync(int systemid);

        Task<bool> ExistsAsync(int systemid);
    }
}
