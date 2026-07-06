/*
 * TRANSFORMENGINE MIGRATION — AccessUserLevelRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IAccessUserLevelRepository
 *   - All read operations use AsNoTracking for performance
 *   - Three-column composite PK (systemid, ntlogin, accesslevelid) — used in all key-based operations
 *   - GetBySystemIdAsync supports Admin Maintenance Tab User Access grid for a given system
 *   - GetByUserAsync supports listing all level assignments for a specific user (systemid + ntlogin)
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete on three-column composite PK
 *   - No UpdateAsync — user-level assignments are add/delete only (junction table)
 *
 * PRESERVED:
 *   - All method signatures defined in IAccessUserLevelRepository (Phase 2)
 *   - mabarchive.tblaccessusers_levels is the backing table (mapped via AccessUserLevelMap.cs)
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
    // TRANSFORMENGINE: implements IAccessUserLevelRepository — backs mabarchive.tblaccessusers_levels; three-column composite PK (systemid, ntlogin, accesslevelid)
    public class AccessUserLevelRepository : BaseRepository, IAccessUserLevelRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserLevelRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full user-level assignment list across all systems
        public async Task<List<AccessUserLevel>> GetAllAsync()
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .OrderBy(ul => ul.Systemid)
                .ThenBy(ul => ul.Ntlogin)
                .ThenBy(ul => ul.Accesslevelid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by systemid — Admin Maintenance Tab User Access grid per system
        public async Task<List<AccessUserLevel>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.Systemid == systemid)
                .OrderBy(ul => ul.Ntlogin)
                .ThenBy(ul => ul.Accesslevelid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by (systemid, ntlogin) — all level assignments for a given user
        public async Task<List<AccessUserLevel>> GetByUserAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.Systemid == systemid && ul.Ntlogin == ntlogin)
                .OrderBy(ul => ul.Accesslevelid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by three-column composite PK
        public async Task<AccessUserLevel?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(ul => ul.Systemid == systemid
                                        && ul.Ntlogin == ntlogin
                                        && ul.Accesslevelid == accesslevelid);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync; junction table — no surrogate key
        public async Task<AccessUserLevel> AddAsync(AccessUserLevel entity)
        {
            _dbContext.AccessUserLevels.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on all three PK columns
        public async Task DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            await _dbContext.AccessUserLevels
                .Where(ul => ul.Systemid == systemid
                          && ul.Ntlogin == ntlogin
                          && ul.Accesslevelid == accesslevelid)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on three-column composite PK
        public async Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid)
        {
            return await _dbContext.AccessUserLevels
                .AnyAsync(ul => ul.Systemid == systemid
                             && ul.Ntlogin == ntlogin
                             && ul.Accesslevelid == accesslevelid);
        }
    }
}
