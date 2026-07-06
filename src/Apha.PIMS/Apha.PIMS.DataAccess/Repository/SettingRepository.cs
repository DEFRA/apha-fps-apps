/*
 * TRANSFORMENGINE MIGRATION — SettingRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing ISettingRepository
 *   - All read operations use AsNoTracking for performance
 *   - PK is string (id) — all key-based operations use string equality
 *   - GetAllUserUpdateableAsync filters on Userupdateable == true (user-editable settings only)
 *   - No AddAsync / DeleteAsync — settings are pre-seeded configuration records
 *   - UpdateAsync uses EF Update + SaveChangesAsync
 *
 * PRESERVED:
 *   - All method signatures defined in ISettingRepository (Phase 2)
 *   - mabarchive.tbl_settings is the backing table (mapped via SettingMap.cs)
 *   - Testsetting column preserved for environment-conditional editing
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SettingService (Phase 3) should enforce Userupdateable guard before calling UpdateAsync
 *   - TRANSFORMENGINE TODO: Testsetting environment-conditional editing guard must be at service/controller layer
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements ISettingRepository — backs mabarchive.tbl_settings; string PK; no add/delete (pre-seeded config)
    public class SettingRepository : BaseRepository, ISettingRepository
    {
        private readonly PimsDbContext _dbContext;

        public SettingRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — all settings for admin listing
        public async Task<List<Setting>> GetAllAsync()
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered — only user-editable settings (Userupdateable == true); supports Time Tab settings form
        public async Task<List<Setting>> GetAllUserUpdateableAsync()
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .Where(s => s.Userupdateable)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by string PK
        public async Task<Setting?> GetByIdAsync(string id)
        {
            return await _dbContext.Settings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        // TRANSFORMENGINE: update only — EF Update + SaveChangesAsync; no add/delete for settings
        public async Task<Setting> UpdateAsync(Setting entity)
        {
            _dbContext.Settings.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: AnyAsync guard on string PK
        public async Task<bool> ExistsAsync(string id)
        {
            return await _dbContext.Settings
                .AnyAsync(s => s.Id == id);
        }
    }
}
