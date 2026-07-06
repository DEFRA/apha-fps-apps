/*
 * TRANSFORMENGINE MIGRATION — AccessLevelRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IAccessLevelRepository
 *   - All read operations use AsNoTracking for performance
 *   - Composite PK (systemid, accesslevelid) — used in all key-based operations
 *   - GetBySystemIdAsync supports Admin Maintenance Tab Access Level dropdown per system
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete on composite PK
 *
 * PRESERVED:
 *   - All method signatures defined in IAccessLevelRepository (Phase 2)
 *   - mabarchive.tblaccesslevels is the backing table (mapped via AccessLevelMap.cs)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements IAccessLevelRepository — backs mabarchive.tblaccesslevels; composite PK (systemid, accesslevelid)
    public class AccessLevelRepository : BaseRepository, IAccessLevelRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessLevelRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full level list across all systems
        public async Task<List<AccessLevel>> GetAllAsync()
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .OrderBy(l => l.Systemid)
                .ThenBy(l => l.Accesslevelid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by systemid — Admin Maintenance Tab Access Level dropdown per system
        public async Task<List<AccessLevel>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .Where(l => l.Systemid == systemid)
                .OrderBy(l => l.Accesslevelid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by composite PK (systemid, accesslevelid)
        public async Task<AccessLevel?> GetByIdAsync(int systemid, int accesslevelid)
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Systemid == systemid && l.Accesslevelid == accesslevelid);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync
        public async Task<AccessLevel> AddAsync(AccessLevel entity)
        {
            _dbContext.AccessLevels.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: update — EF Update + SaveChangesAsync
        public async Task<AccessLevel> UpdateAsync(AccessLevel entity)
        {
            _dbContext.AccessLevels.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on composite PK (systemid, accesslevelid)
        public async Task DeleteAsync(int systemid, int accesslevelid)
        {
            await _dbContext.AccessLevels
                .Where(l => l.Systemid == systemid && l.Accesslevelid == accesslevelid)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on composite PK (systemid, accesslevelid)
        public async Task<bool> ExistsAsync(int systemid, int accesslevelid)
        {
            return await _dbContext.AccessLevels
                .AnyAsync(l => l.Systemid == systemid && l.Accesslevelid == accesslevelid);
        }
    }
}
