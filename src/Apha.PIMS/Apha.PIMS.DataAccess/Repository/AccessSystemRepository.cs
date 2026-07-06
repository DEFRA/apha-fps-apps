/*
 * TRANSFORMENGINE MIGRATION — AccessSystemRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IAccessSystemRepository
 *   - All read operations use AsNoTracking for performance
 *   - Single integer PK (systemid) — lookup/reference table, read-only from PIMS perspective
 *   - Only GetAllAsync, GetByIdAsync, ExistsAsync — no add/update/delete (system records managed externally)
 *
 * PRESERVED:
 *   - All method signatures defined in IAccessSystemRepository (Phase 2)
 *   - mabarchive.tblaccesssystems is the backing table (mapped via AccessSystemMap.cs)
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
    // TRANSFORMENGINE: implements IAccessSystemRepository — backs mabarchive.tblaccesssystems; single integer PK (systemid); lookup/reference table
    public class AccessSystemRepository : BaseRepository, IAccessSystemRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessSystemRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full system list for system dropdown / filter lookup
        public async Task<List<AccessSystem>> GetAllAsync()
        {
            return await _dbContext.AccessSystems
                .AsNoTracking()
                .OrderBy(s => s.Systemid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by integer PK
        public async Task<AccessSystem?> GetByIdAsync(int systemid)
        {
            return await _dbContext.AccessSystems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Systemid == systemid);
        }

        // TRANSFORMENGINE: AnyAsync guard — avoids full row load for existence check
        public async Task<bool> ExistsAsync(int systemid)
        {
            return await _dbContext.AccessSystems
                .AnyAsync(s => s.Systemid == systemid);
        }
    }
}
