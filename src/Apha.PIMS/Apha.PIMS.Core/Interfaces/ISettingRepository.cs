/*
 * TRANSFORMENGINE MIGRATION — ISettingRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for Setting read/update operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - PK is string (id) — reflected in GetByIdAsync / ExistsAsync
 *   - GetAllAsync supports admin listing of all application settings
 *   - GetAllUserUpdateableAsync supports filtered view of user-editable settings only
 *   - No DeleteAsync / AddAsync — settings are configuration records, not user-created entities
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
    // TRANSFORMENGINE: interface covers read/update for Setting (mabarchive.tbl_settings); string PK; no add/delete (settings are pre-seeded config)
    public interface ISettingRepository
    {
        Task<List<Setting>> GetAllAsync();

        Task<List<Setting>> GetAllUserUpdateableAsync();

        Task<Setting?> GetByIdAsync(string id);

        Task<Setting> UpdateAsync(Setting entity);

        Task<bool> ExistsAsync(string id);
    }
}
