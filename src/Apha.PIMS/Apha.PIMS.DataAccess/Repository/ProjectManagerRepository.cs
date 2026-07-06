/*
 * TRANSFORMENGINE MIGRATION — ProjectManagerRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IProjectManagerRepository
 *   - All read operations use AsNoTracking for performance
 *   - PK is string (projectmanager name) — all key-based operations use string equality
 *   - ExistsAsync uses AnyAsync guard pattern
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete
 *
 * PRESERVED:
 *   - All method signatures defined in IProjectManagerRepository (Phase 2)
 *   - tblprojectmanager is the backing table (mapped via ProjectManagerMap.cs)
 *   - Disable flag preserved for soft-disable support
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
    // TRANSFORMENGINE: implements IProjectManagerRepository — backs tblprojectmanager; string PK (projectmanager)
    public class ProjectManagerRepository : BaseRepository, IProjectManagerRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProjectManagerRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full manager list ordered by name; supports Manager Tab grid
        public async Task<List<ProjectManager>> GetAllAsync()
        {
            return await _dbContext.ProjectManagers
                .AsNoTracking()
                .OrderBy(m => m.Projectmanager)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by string PK
        public async Task<ProjectManager?> GetByIdAsync(string projectmanager)
        {
            return await _dbContext.ProjectManagers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Projectmanager == projectmanager);
        }

        // TRANSFORMENGINE: insert — EF Add + SaveChangesAsync; string PK supplied by caller
        public async Task<ProjectManager> AddAsync(ProjectManager entity)
        {
            _dbContext.ProjectManagers.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: update — EF Update + SaveChangesAsync
        public async Task<ProjectManager> UpdateAsync(ProjectManager entity)
        {
            _dbContext.ProjectManagers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: set-based delete via ExecuteDeleteAsync on string PK
        public async Task DeleteAsync(string projectmanager)
        {
            await _dbContext.ProjectManagers
                .Where(m => m.Projectmanager == projectmanager)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard on string PK
        public async Task<bool> ExistsAsync(string projectmanager)
        {
            return await _dbContext.ProjectManagers
                .AnyAsync(m => m.Projectmanager == projectmanager);
        }
    }
}
