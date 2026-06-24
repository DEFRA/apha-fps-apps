/*
 * TRANSFORMENGINE MIGRATION — IWorkgroupService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service interface created; no prior IWorkgroupService existed in this codebase
 *   - Interface designed from frmMaintWorkGroup2 CRUD operations and IWorkgroupRepository surface
 *   - Lookup methods (GetAllProfitCentresAsync, GetOwnersAsync, GetCostCentresByProfitCentreAsync)
 *     mirror the three dropdown endpoints identified in the backend handoff notes
 *   - GetPagedAsync uses QueryParameters<string> consistent with all other FPS service interfaces
 *
 * PRESERVED:
 *   - Application layer purity: no DbContext or infrastructure references
 *   - Async-only signatures consistent with IWorkGroupGradeService and sibling interfaces
 *   - DTO-only return types — no entity types cross the application/domain boundary
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetOwnersAsync returns ManagerDto — confirm qryManager result shape
 *     matches Manager entity fields (Name, WorkGroup, GradeCode, Expr1)
 *   - TRANSFORMENGINE TODO: GetCostCentresByProfitCentreAsync returns double? list —
 *     confirm whether a richer CostCentreLookupDto is needed by the frontend
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Workgroup CRUD and lookup operations
    /// (frmMaintWorkGroup2 — fps.workgroup table).
    /// </summary>
    public interface IWorkgroupService
    {
        // TRANSFORMENGINE: GetPagedAsync — paged list with optional search/sort filter
        /// <summary>Returns a paginated, optionally filtered list of workgroups for the active FPS year.</summary>
        Task<PaginatedResult<WorkgroupDto>> GetPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GetByKeyAsync — single workgroup by WorkGroupName
        /// <summary>Returns a single workgroup by WorkGroupName, or null if not found.</summary>
        Task<WorkgroupDto?> GetByKeyAsync(string workGroupName);

        // TRANSFORMENGINE: CreateAsync — insert workgroup; duplicate name check performed in implementation
        /// <summary>Creates a new workgroup record. Throws ArgumentException if a workgroup with the same name already exists.</summary>
        Task<WorkgroupDto> CreateAsync(WorkgroupDto dto);

        // TRANSFORMENGINE: UpdateAsync — update workgroup; supports optional WorkGroupName rename via originalWorkGroupName
        /// <summary>
        /// Updates an existing workgroup identified by <paramref name="originalWorkGroupName"/>.
        /// Pass the same value as <c>dto.WorkGroupName</c> when no rename is required.
        /// Throws KeyNotFoundException if the workgroup does not exist.
        /// </summary>
        Task<WorkgroupDto> UpdateAsync(string originalWorkGroupName, WorkgroupDto dto);

        // TRANSFORMENGINE: DeleteAsync — remove workgroup; throws if not found
        /// <summary>Deletes the workgroup with the given WorkGroupName. Returns true if deleted, false if not found.</summary>
        Task<bool> DeleteAsync(string workGroupName);

        // TRANSFORMENGINE: GetAllProfitCentresAsync — ProfitCentre dropdown; distinct values from tblkpprofitcentre
        /// <summary>Returns all available profit centre identifiers for the ProfitCentre dropdown.</summary>
        Task<IEnumerable<string>> GetAllProfitCentresAsync();

        // TRANSFORMENGINE: GetOwnersAsync — Owner dropdown; maps to qryManager named query (Manager entity)
        /// <summary>Returns all manager records for the Owner dropdown.</summary>
        Task<IEnumerable<ManagerDto>> GetOwnersAsync();

        // TRANSFORMENGINE: GetCostCentresByProfitCentreAsync — cascading CostCentre dropdown filtered by ProfitCentre
        /// <summary>
        /// Returns cost centre values linked to the given <paramref name="profitCentre"/>,
        /// for use in the CostCentre cascading dropdown.
        /// </summary>
        Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre);
    }
}
