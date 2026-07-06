/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreManagerLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IProfitCentreManagerLinkRepository
 *   - All read operations use AsNoTracking for performance
 *   - Composite PK (profitcentre, manager) — both string — used throughout
 *   - GetByProfitCentreAsync enables sub-grid population for a given profit centre
 *   - DeleteAsync and ExistsAsync filter on both PK columns
 *
 * PRESERVED:
 *   - All method signatures defined in IProfitCentreManagerLinkRepository (Phase 2)
 *   - mabarchive.tblprofitcentre_manager_link is the backing table (mapped via ProfitCentreManagerLinkMap.cs)
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
    // TRANSFORMENGINE: implements IProfitCentreManagerLinkRepository — backs mabarchive.tblprofitcentre_manager_link; composite PK (profitcentre, manager)
    public class ProfitCentreManagerLinkRepository : BaseRepository, IProfitCentreManagerLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProfitCentreManagerLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking full list — all profit centre/manager link records
        public async Task<List<ProfitCentreManagerLink>> GetAllAsync()
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .OrderBy(l => l.Profitcentre)
                .ThenBy(l => l.Manager)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by profitcentre — supports Manager Tab Resource Centre sub-grid
        public async Task<List<ProfitCentreManagerLink>> GetByProfitCentreAsync(string profitcentre)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .Where(l => l.Profitcentre == profitcentre)
                .OrderBy(l => l.Manager)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by composite PK (profitcentre, manager)
        public async Task<ProfitCentreManagerLink?> GetByIdAsync(string profitcentre, string manager)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Profitcentre == profitcentre && l.Manager == manager);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync; no surrogate key
        public async Task<ProfitCentreManagerLink> AddAsync(ProfitCentreManagerLink entity)
        {
            _dbContext.ProfitCentreManagerLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on both PK columns
        public async Task DeleteAsync(string profitcentre, string manager)
        {
            await _dbContext.ProfitCentreManagerLinks
                .Where(l => l.Profitcentre == profitcentre && l.Manager == manager)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on composite PK (profitcentre, manager)
        public async Task<bool> ExistsAsync(string profitcentre, string manager)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AnyAsync(l => l.Profitcentre == profitcentre && l.Manager == manager);
        }
    }
}
