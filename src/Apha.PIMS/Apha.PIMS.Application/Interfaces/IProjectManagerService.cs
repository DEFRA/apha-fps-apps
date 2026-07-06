/*
 * TRANSFORMENGINE MIGRATION — IProjectManagerService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ProjectManager CRUD operations (Manager Tab, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - String PK (projectmanager name) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetAllAsync returns full list for manager lookup/dropdown
 *
 * PRESERVED:
 *   - No infrastructure-specific code in this Application interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    // TRANSFORMENGINE: service interface for ProjectManager CRUD; string PK; consumed by ProjectManagerController (Phase 5)
    public interface IProjectManagerService
    {
        Task<List<ProjectManagerDto>> GetAllAsync();

        Task<ProjectManagerDto?> GetByIdAsync(string projectmanager);

        Task<ProjectManagerDto> CreateAsync(ProjectManagerDto dto);

        Task<ProjectManagerDto> UpdateAsync(ProjectManagerDto dto);

        Task DeleteAsync(string projectmanager);

        Task<bool> ExistsAsync(string projectmanager);
    }
}
