/*
 * TRANSFORMENGINE MIGRATION — ICostCentreRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres CRUD operations (saveTblCostCentre, updateTblCostCentre, handleTblCostCentreDelete) → async repository interface
 *   - DataGrid paged list (GET paged) → GetAllPagedAsync with PaginationParameters
 *   - Single-record lookup for Edit modal (GET by id) → GetByIdAsync with composite key (costCentreNo, fpsYear)
 *   - Add/Create action → CreateAsync
 *   - Update action → UpdateAsync with original key + updated entity
 *   - Delete action → DeleteAsync returning bool success flag
 *   - Existence guard for duplicate-prevention → ExistsAsync
 *
 * PRESERVED:
 *   - Composite PK semantics from DDL: (costcentre, fpsyear) — both parameters required for key lookups
 *   - No infrastructure-specific types (no DbContext, EF, or SQL); Core layer stays clean
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If FK-violation checks are needed before delete (e.g. child rows), add HasLinkedXxxAsync methods similar to IProfitCentreRepository.
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for CostCentre CRUD and paged-list operations against fps.costcentre.
    /// </summary>
    public interface ICostCentreRepository
    {
        // TRANSFORMENGINE: GET paged — drives DataGrid in fps_costcenter_maintenance.html (#gridContainer_costcenterList)
        /// <summary>Returns a paginated list of CostCentre records for the maintenance grid.</summary>
        Task<PagedData<CostCentre>> GetAllPagedAsync(PaginationParameters<string> query);

        // TRANSFORMENGINE: GET by composite key — populates Edit modal fields (modal-cc-number, modal-cc-profit)
        /// <summary>Returns a single CostCentre by its composite key (costCentreNo + fpsYear), or null if not found.</summary>
        Task<CostCentre?> GetByIdAsync(double costCentreNo, int fpsYear);

        // TRANSFORMENGINE: POST create — maps to saveTblCostCentre() in costcenter_maintenance.js
        /// <summary>Inserts a new CostCentre record and returns the persisted entity.</summary>
        Task<CostCentre> CreateAsync(CostCentre entity);

        // TRANSFORMENGINE: PUT update — maps to updateTblCostCentre() in costcenter_maintenance.js
        /// <summary>Updates an existing CostCentre record identified by originalCostCentreNo + fpsYear and returns the updated entity.</summary>
        Task<CostCentre> UpdateAsync(double originalCostCentreNo, int fpsYear, CostCentre entity);

        // TRANSFORMENGINE: DELETE — maps to handleTblCostCentreDelete() in costcenter_maintenance.js
        /// <summary>Deletes the CostCentre row for the given composite key. Returns true if a row was deleted.</summary>
        Task<bool> DeleteAsync(double costCentreNo, int fpsYear);

        // TRANSFORMENGINE: Existence check — prevents duplicate-key insert at the service layer before persisting
        /// <summary>Returns true if a CostCentre row with the given composite key already exists.</summary>
        Task<bool> ExistsAsync(double costCentreNo, int fpsYear);
    }
}
