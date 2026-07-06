/*
 * TRANSFORMENGINE MIGRATION — ProgramManagerLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IProgramManagerLinkRepository
 *   - All read operations use AsNoTracking for performance
 *   - Composite PK (program, manager) — both string — used throughout
 *   - GetByProgramAsync enables sub-grid population for a given program
 *   - DeleteAsync and ExistsAsync filter on both PK columns
 *
 * PRESERVED:
 *   - All method signatures defined in IProgramManagerLinkRepository (Phase 2)
 *   - mabarchive.tblprogram_manager_link is the backing table (mapped via ProgramManagerLinkMap.cs)
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
    // TRANSFORMENGINE: implements IProgramManagerLinkRepository — backs mabarchive.tblprogram_manager_link; composite PK (program, manager)
    public class ProgramManagerLinkRepository : BaseRepository, IProgramManagerLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProgramManagerLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking full list — all program/manager link records
        public async Task<List<ProgramManagerLink>> GetAllAsync()
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .OrderBy(l => l.Program)
                .ThenBy(l => l.Manager)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by program — supports Manager Tab Program sub-grid
        public async Task<List<ProgramManagerLink>> GetByProgramAsync(string program)
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .Where(l => l.Program == program)
                .OrderBy(l => l.Manager)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by composite PK (program, manager)
        public async Task<ProgramManagerLink?> GetByIdAsync(string program, string manager)
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Program == program && l.Manager == manager);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync; no surrogate key
        public async Task<ProgramManagerLink> AddAsync(ProgramManagerLink entity)
        {
            _dbContext.ProgramManagerLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on both PK columns
        public async Task DeleteAsync(string program, string manager)
        {
            await _dbContext.ProgramManagerLinks
                .Where(l => l.Program == program && l.Manager == manager)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on composite PK (program, manager)
        public async Task<bool> ExistsAsync(string program, string manager)
        {
            return await _dbContext.ProgramManagerLinks
                .AnyAsync(l => l.Program == program && l.Manager == manager);
        }
    }
}
