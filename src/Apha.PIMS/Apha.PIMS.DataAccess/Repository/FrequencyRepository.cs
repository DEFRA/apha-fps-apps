/*
 * TRANSFORMENGINE MIGRATION — FrequencyRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IFrequencyRepository
 *   - All read operations use AsNoTracking for performance
 *   - Single integer PK (frequencyid) — ValueGeneratedNever, client-supplied on create
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete (no entity load required)
 *   - AddAsync uses Add + SaveChangesAsync; UpdateAsync uses Update + SaveChangesAsync
 *   - ExistsAsync uses AnyAsync guard — avoids full row load
 *
 * PRESERVED:
 *   - All method signatures defined in IFrequencyRepository (Phase 2)
 *   - mabarchive.tlkpfrequency is the backing table (mapped via FrequencyMap.cs)
 *   - Frequency.FrequencyValue property alias (DDL column 'frequency' → FrequencyValue)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer PK generation strategy; if DB auto-generates, update Req contract and remove client-supplied PK pattern
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements IFrequencyRepository — backs mabarchive.tlkpfrequency; single integer PK (frequencyid); lookup/reference table with CRUD
    public class FrequencyRepository : BaseRepository, IFrequencyRepository
    {
        private readonly PimsDbContext _dbContext;

        public FrequencyRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full frequency list for dropdown / lookup usage
        public async Task<List<Frequency>> GetAllAsync()
        {
            return await _dbContext.Frequencies
                .AsNoTracking()
                .OrderBy(f => f.Frequencyid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by integer PK (frequencyid)
        public async Task<Frequency?> GetByIdAsync(int frequencyid)
        {
            return await _dbContext.Frequencies
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Frequencyid == frequencyid);
        }

        // TRANSFORMENGINE: Add + SaveChangesAsync — PK is client-supplied (ValueGeneratedNever on tlkpfrequency.frequencyid)
        public async Task<Frequency> AddAsync(Frequency entity)
        {
            _dbContext.Frequencies.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: Update + SaveChangesAsync — replaces full row for the supplied entity
        public async Task<Frequency> UpdateAsync(Frequency entity)
        {
            _dbContext.Frequencies.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: ExecuteDeleteAsync — set-based delete by integer PK; no entity load required
        public async Task DeleteAsync(int frequencyid)
        {
            await _dbContext.Frequencies
                .Where(f => f.Frequencyid == frequencyid)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard — avoids full row load for existence check
        public async Task<bool> ExistsAsync(int frequencyid)
        {
            return await _dbContext.Frequencies
                .AnyAsync(f => f.Frequencyid == frequencyid);
        }
    }
}
