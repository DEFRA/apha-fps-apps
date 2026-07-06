/*
 * TRANSFORMENGINE MIGRATION — ReportGroupLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IReportGroupLinkRepository
 *   - All read operations use AsNoTracking for performance
 *   - Composite PK (reportid, groupid) used throughout — no surrogate key
 *   - DeleteAsync and ExistsAsync filter on both PK columns
 *   - GetByReportIdAsync enables sub-grid population for a given report
 *
 * PRESERVED:
 *   - All method signatures defined in IReportGroupLinkRepository (Phase 2)
 *   - mabarchive.tblreportgroup_link is the backing table (mapped via ReportGroupLinkMap.cs)
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
    // TRANSFORMENGINE: implements IReportGroupLinkRepository — backs mabarchive.tblreportgroup_link; composite PK (reportid, groupid)
    public class ReportGroupLinkRepository : BaseRepository, IReportGroupLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReportGroupLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking full list — all report/group link records
        public async Task<List<ReportGroupLink>> GetAllAsync()
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .OrderBy(l => l.Reportid)
                .ThenBy(l => l.Groupid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking filtered by reportid — supports report sub-grid population
        public async Task<List<ReportGroupLink>> GetByReportIdAsync(int reportid)
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .Where(l => l.Reportid == reportid)
                .OrderBy(l => l.Groupid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by composite PK (reportid, groupid)
        public async Task<ReportGroupLink?> GetByIdAsync(int reportid, int groupid)
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Reportid == reportid && l.Groupid == groupid);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync; no surrogate key
        public async Task<ReportGroupLink> AddAsync(ReportGroupLink entity)
        {
            _dbContext.ReportGroupLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync — filters on both PK columns
        public async Task DeleteAsync(int reportid, int groupid)
        {
            await _dbContext.ReportGroupLinks
                .Where(l => l.Reportid == reportid && l.Groupid == groupid)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on composite PK (reportid, groupid)
        public async Task<bool> ExistsAsync(int reportid, int groupid)
        {
            return await _dbContext.ReportGroupLinks
                .AnyAsync(l => l.Reportid == reportid && l.Groupid == groupid);
        }
    }
}
