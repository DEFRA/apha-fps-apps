/*
 * TRANSFORMENGINE MIGRATION — AccessUserRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IAccessUserRepository
 *   - All read operations use AsNoTracking for performance
 *   - Composite PK (systemid, ntlogin) — used in all key-based operations
 *   - GetBySystemIdAsync supports Admin Maintenance Tab Users grid per system
 *   - GetByNtLoginAsync supports cross-system user lookup by NT login
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete on composite PK
 *
 * PRESERVED:
 *   - All method signatures defined in IAccessUserRepository (Phase 2)
 *   - mabarchive.tblaccessusers is the backing table (mapped via AccessUserMap.cs)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: systemid scope at service/controller layer — verify it derives from session, not client payload
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements IAccessUserRepository — backs mabarchive.tblaccessusers; composite PK (systemid, ntlogin)
    public class AccessUserRepository : BaseRepository, IAccessUserRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full user list across all systems
        public async Task<List<AccessUser>> GetAllAsync()
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .OrderBy(u => u.Systemid)
                .ThenBy(u => u.Ntlogin)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by systemid — Admin Maintenance Tab Users grid per access system
        public async Task<List<AccessUser>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.Systemid == systemid)
                .OrderBy(u => u.Ntlogin)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by ntlogin — cross-system lookup for a given NT login
        public async Task<List<AccessUser>> GetByNtLoginAsync(string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.Ntlogin == ntlogin)
                .OrderBy(u => u.Systemid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by composite PK (systemid, ntlogin)
        public async Task<AccessUser?> GetByIdAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Systemid == systemid && u.Ntlogin == ntlogin);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync
        public async Task<AccessUser> AddAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: update — EF Update + SaveChangesAsync
        public async Task<AccessUser> UpdateAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on composite PK (systemid, ntlogin)
        public async Task DeleteAsync(int systemid, string ntlogin)
        {
            await _dbContext.AccessUsers
                .Where(u => u.Systemid == systemid && u.Ntlogin == ntlogin)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on composite PK (systemid, ntlogin)
        public async Task<bool> ExistsAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AnyAsync(u => u.Systemid == systemid && u.Ntlogin == ntlogin);
        }
    }
}
