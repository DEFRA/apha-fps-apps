/*
 * TRANSFORMENGINE MIGRATION — IProjectManagerRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ProjectManager CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - PK is string (Projectmanager name) — reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetAllAsync returns full list for manager lookup/dropdown usage
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) in this Core interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: interface covers CRUD for ProjectManager; string PK (projectmanager name)
    public interface IProjectManagerRepository
    {
        Task<List<ProjectManager>> GetAllAsync();

        Task<ProjectManager?> GetByIdAsync(string projectmanager);

        Task<ProjectManager> AddAsync(ProjectManager entity);

        Task<ProjectManager> UpdateAsync(ProjectManager entity);

        Task DeleteAsync(string projectmanager);

        Task<bool> ExistsAsync(string projectmanager);
    }
}
