/*
 * TRANSFORMENGINE MIGRATION — RadTrackProgRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IRadTrackProgRepository
 *   - All read operations use AsNoTracking for performance
 *   - Natural string PK (program varchar(10)) — client-supplied on create
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete (no entity load required)
 *   - AddAsync uses Add + SaveChangesAsync; UpdateAsync uses Update + SaveChangesAsync
 *   - ExistsAsync uses AnyAsync guard — avoids full row load
 *
 * PRESERVED:
 *   - All method signatures defined in IRadTrackProgRepository
 *   - mabarchive.tblradtrackprog is the backing table (mapped via RadtrackProgMap.cs)
 *   - RadtrackProg entity property naming convention preserved
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
    // TRANSFORMENGINE: implements IRadTrackProgRepository — backs mabarchive.tblradtrackprog; natural string PK (program varchar(10)); Programme Tab
    public class RadTrackProgRepository : BaseRepository, IRadTrackProgRepository
    {
        private readonly PimsDbContext _dbContext;

        public RadTrackProgRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full programme list ordered by PK
        public async Task<List<RadtrackProg>> GetAllAsync()
        {
            return await _dbContext.RadtrackProgs
                .AsNoTracking()
                .OrderBy(r => r.Program)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by natural string PK (program)
        public async Task<RadtrackProg?> GetByIdAsync(string program)
        {
            return await _dbContext.RadtrackProgs
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Program == program);
        }

        // TRANSFORMENGINE: Add + SaveChangesAsync — PK is client-supplied (natural varchar key)
        public async Task<RadtrackProg> AddAsync(RadtrackProg entity)
        {
            _dbContext.RadtrackProgs.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: Update + SaveChangesAsync — replaces full row for the supplied entity
        public async Task<RadtrackProg> UpdateAsync(RadtrackProg entity)
        {
            _dbContext.RadtrackProgs.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: ExecuteDeleteAsync — set-based delete by natural string PK; no entity load required
        public async Task DeleteAsync(string program)
        {
            await _dbContext.RadtrackProgs
                .Where(r => r.Program == program)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard — avoids full row load for existence check
        public async Task<bool> ExistsAsync(string program)
        {
            return await _dbContext.RadtrackProgs
                .AnyAsync(r => r.Program == program);
        }
    }
}
