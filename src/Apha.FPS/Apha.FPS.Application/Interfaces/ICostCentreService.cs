/*
 * TRANSFORMENGINE MIGRATION — ICostCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres CRUD VBA callbacks (saveTblCostCentre, updateTblCostCentre, handleTblCostCentreDelete) → async service interface
 *   - DataGrid paged list → GetAllCostCentresPagedAsync(QueryParameters<string>)
 *   - Edit modal lookup → GetCostCentreByIdAsync(double costCentreNo, int fpsYear)
 *   - Add/Create action → CreateCostCentreAsync(CostCentreDto)
 *   - Update action → UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto)
 *   - Delete action → DeleteCostCentreAsync(double costCentreNo, int fpsYear)
 *
 * PRESERVED:
 *   - Composite key semantics from DDL: (costcentre, fpsyear) — both parameters required for key operations
 *   - No infrastructure types exposed; interface stays within Application layer contracts
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If a lookup-only GetAllCostCentresAsync (non-paged) is needed for dropdowns, add it here and in the implementation.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for CostCentre CRUD and paged-list operations.
    /// Orchestrates business logic (FK validation, duplicate detection) on top of ICostCentreRepository.
    /// </summary>
    public interface ICostCentreService
    {
        // TRANSFORMENGINE: GET paged — drives DataGrid in fps_costcenter_maintenance.html (#gridContainer_costcenterList)
        /// <summary>Returns a paginated list of CostCentre records for the maintenance grid.</summary>
        Task<PaginatedResult<CostCentreDto>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GET by composite key — populates Edit modal fields (modal-cc-number, modal-cc-profit)
        /// <summary>Returns a single CostCentre by its composite key (costCentreNo + fpsYear), or null if not found.</summary>
        Task<CostCentreDto?> GetCostCentreByIdAsync(double costCentreNo, int fpsYear);

        // TRANSFORMENGINE: POST create — maps to saveTblCostCentre() in costcenter_maintenance.js; validates ProfitCentre FK
        /// <summary>
        /// Validates that the ProfitCentre exists and the composite key is not a duplicate, then inserts and returns the persisted DTO.
        /// Throws <see cref="ArgumentNullException"/> if dto is null.
        /// Throws <see cref="InvalidOperationException"/> if the key already exists or ProfitCentre FK is invalid.
        /// </summary>
        Task<CostCentreDto> CreateCostCentreAsync(CostCentreDto costCentreDto);

        // TRANSFORMENGINE: PUT update — maps to updateTblCostCentre() in costcenter_maintenance.js; validates ProfitCentre FK
        /// <summary>
        /// Validates that the original record exists and the new ProfitCentre FK is valid, then updates and returns the updated DTO.
        /// Throws <see cref="ArgumentNullException"/> if dto is null.
        /// Throws <see cref="KeyNotFoundException"/> if the original record does not exist.
        /// Throws <see cref="InvalidOperationException"/> if ProfitCentre FK is invalid.
        /// </summary>
        Task<CostCentreDto> UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto costCentreDto);

        // TRANSFORMENGINE: DELETE — maps to handleTblCostCentreDelete() in costcenter_maintenance.js
        /// <summary>
        /// Deletes the CostCentre row for the given composite key.
        /// Throws <see cref="KeyNotFoundException"/> if the record does not exist.
        /// Returns true if a row was deleted.
        /// </summary>
        Task<bool> DeleteCostCentreAsync(double costCentreNo, int fpsYear);
    }
}
