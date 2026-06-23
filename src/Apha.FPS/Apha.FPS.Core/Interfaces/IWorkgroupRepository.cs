/*
 * TRANSFORMENGINE MIGRATION — IWorkgroupRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New interface created; no prior IWorkgroupRepository existed in this codebase
 *   - Interface designed from frmMaintWorkGroup2 VBA CRUD operations and planned
 *     WorkgroupController route surface (Phase 5 plan notes)
 *   - Composite primary key (WorkGroupName + FpsYear) reflected in all key-based
 *     operations; FpsYear is resolved via the DbContext HasQueryFilter so callers
 *     only supply WorkGroupName
 *   - Lookup methods added for the three dropdown endpoints identified in the
 *     backend handoff notes: ProfitCentres, Owners (qryManager), CostCentres
 *
 * PRESERVED:
 *   - Core layer purity: no DbContext, EF, or infrastructure references
 *   - Async-only signatures consistent with all other repository interfaces in this project
 *   - Return types aligned with existing entity and pagination model shapes already in Core
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetOwnersAsync returns Manager entities — confirm that the
 *     qryManager named query result shape matches Manager.cs fields (Name, WorkGroup,
 *     GradeCode, Expr1) before wiring the repository implementation
 *   - TRANSFORMENGINE TODO: GetCostCentresByProfitCentreAsync — CostCentre column is
 *     double? in the DDL; confirm the return type (double?) is sufficient or whether a
 *     dedicated CostCentreLookup projection is needed for the dropdown
 *   - TRANSFORMENGINE TODO: WorkGroupName rename support in UpdateAsync — verify whether
 *     the legacy form allowed renaming the primary key; if not, originalWorkGroupName
 *     param can be collapsed to a single entity param
 */
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for async CRUD, paged query, and lookup operations on <see cref="Workgroup"/>.
    /// Implementations must respect the FpsYear query filter applied by FpsDbContext.
    /// Composite PK is (WorkGroupName, FpsYear); FpsYear is resolved automatically by the
    /// active-year filter so callers supply WorkGroupName only.
    /// </summary>
    public interface IWorkgroupRepository
    {
        // TRANSFORMENGINE: GetPagedAsync — paged list with optional search/sort; string filter covers WorkGroupName + ProfitCentre + Description
        /// <summary>Returns a paged, optionally filtered and sorted list of workgroups for the active FPS year.</summary>
        Task<PagedData<Workgroup>> GetPagedAsync(PaginationParameters<string> query);

        // TRANSFORMENGINE: GetByKeyAsync — look up single workgroup by WorkGroupName; FpsYear resolved via DbContext HasQueryFilter
        /// <summary>Returns a single workgroup by its WorkGroupName, or null if not found in the active FPS year.</summary>
        Task<Workgroup?> GetByKeyAsync(string workGroupName);

        // TRANSFORMENGINE: CreateAsync — insert new workgroup record for the active FPS year
        /// <summary>Inserts a new workgroup record and returns the persisted entity.</summary>
        Task<Workgroup> CreateAsync(Workgroup workgroup);

        // TRANSFORMENGINE: UpdateAsync — update existing workgroup; originalWorkGroupName supports rename if the PK value changes
        /// <summary>
        /// Updates an existing workgroup identified by <paramref name="originalWorkGroupName"/> and returns the updated entity.
        /// Pass the same value as <c>workgroup.WorkGroupName</c> when no rename is needed.
        /// </summary>
        Task<Workgroup> UpdateAsync(string originalWorkGroupName, Workgroup workgroup);

        // TRANSFORMENGINE: DeleteAsync — remove workgroup by WorkGroupName; FpsYear resolved via DbContext HasQueryFilter
        /// <summary>Deletes the workgroup with the given WorkGroupName. Returns true if deleted, false if not found.</summary>
        Task<bool> DeleteAsync(string workGroupName);

        // TRANSFORMENGINE: ExistsAsync — AnyAsync-style existence check used for duplicate validation before Create
        /// <summary>Returns true if a workgroup with the given WorkGroupName exists in the active FPS year.</summary>
        Task<bool> ExistsAsync(string workGroupName);

        // TRANSFORMENGINE: GetAllProfitCentresAsync — lookup dropdown; returns distinct ProfitCentre values from tblkpprofitcentre via FK
        /// <summary>Returns all available profit centre identifiers for the workgroup ProfitCentre dropdown.</summary>
        Task<IEnumerable<string>> GetAllProfitCentresAsync();

        // TRANSFORMENGINE: GetOwnersAsync — lookup dropdown; maps to qryManager named query result set (Manager entity)
        /// <summary>Returns all manager records for use as Owner dropdown options.</summary>
        Task<IEnumerable<Manager>> GetOwnersAsync();

        // TRANSFORMENGINE: GetCostCentresByProfitCentreAsync — filtered lookup dropdown; costcentre is double? in fps.workgroup DDL
        /// <summary>
        /// Returns cost centre values linked to the given <paramref name="profitCentre"/>, for use in the
        /// CostCentre dropdown filtered by the selected profit centre.
        /// </summary>
        Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre);
    }
}
