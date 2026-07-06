/*
 * TRANSFORMENGINE MIGRATION — ReportRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IReportRepository
 *   - All read operations use AsNoTracking for performance
 *   - ExistsAsync uses AnyAsync guard pattern
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete (no load-then-delete)
 *   - AddAsync / UpdateAsync use EF change tracking + single SaveChangesAsync
 *
 * PRESERVED:
 *   - All method signatures defined in IReportRepository (Phase 2)
 *   - mabarchive.tblreport is the backing table (mapped via ReportMap.cs)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify 'type' column char(1) usage at service layer
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements IReportRepository — backs mabarchive.tblreport (PostgreSQL)
    public class ReportRepository : BaseRepository, IReportRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReportRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full list for Reports Tab grid
        public async Task<List<Report>> GetAllAsync()
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .OrderBy(r => r.Sortorder)
                .ThenBy(r => r.Reportname)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by integer PK
        public async Task<Report?> GetByIdAsync(int id)
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync
        public async Task<Report> AddAsync(Report entity)
        {
            _dbContext.Reports.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: update — EF Update + SaveChangesAsync
        public async Task<Report> UpdateAsync(Report entity)
        {
            _dbContext.Reports.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — no load-then-delete
        public async Task DeleteAsync(int id)
        {
            await _dbContext.Reports
                .Where(r => r.Id == id)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard — avoids full row load for existence check
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Reports
                .AnyAsync(r => r.Id == id);
        }
    }
}
