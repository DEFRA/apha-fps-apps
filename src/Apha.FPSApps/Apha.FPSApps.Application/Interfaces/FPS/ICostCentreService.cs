/*
 * TRANSFORMENGINE MIGRATION — ICostCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New frontend service interface for cost centre maintenance operations
 *   - Mirrors all 6 methods on IFpsCostCentreApiClient exactly
 *   - Workgroup lookup (GetAllCostCentresAsync) included for dropdown population
 *   - Paged DataGrid (GetAllCostCentresPagedAsync) included for grid binding
 *   - CRUD operations (GetCostCentreByIdAsync, CreateCostCentreAsync, UpdateCostCentreAsync, DeleteCostCentreAsync) included
 *   - double key type (costCentreNo) consistent with backend composite key
 *
 * PRESERVED:
 *   - All return types and parameter types match IFpsCostCentreApiClient signatures exactly
 *   - CostCentreWorkgroupDto used for the workgroup lookup (mirrors backend GetAllCostCentresAsync return)
 *   - QueryParameters<string> for paged list endpoint consistent with other FPS service interfaces
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm that the MVC controller injects ICostCentreService (not the raw API client) — this interface is the only injection point for CostCentreMaintenanceController.
 *   - TRANSFORMENGINE TODO: FpsYear is resolved server-side via X-FPS-Year header; no FpsYear parameter is required here.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for the Cost Centre maintenance resource.
    /// Mirrors all six async methods on <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsCostCentreApiClient"/>.
    /// Injected into <c>CostCentreMaintenanceController</c> in the FPS area.
    /// </summary>
    public interface ICostCentreService
    {
        // TRANSFORMENGINE: workgroup lookup — delegates to IFpsCostCentreApiClient.GetAllCostCentresAsync; used for dropdown population
        /// <summary>
        /// Returns the full list of cost centre workgroup entries for dropdown/lookup population.
        /// </summary>
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync();

        // TRANSFORMENGINE: paginated list — delegates to IFpsCostCentreApiClient.GetAllCostCentresPagedAsync; used for DataGrid binding
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of cost centres for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: single record — delegates to IFpsCostCentreApiClient.GetCostCentreByIdAsync; FpsYear resolved server-side
        /// <summary>
        /// Returns a single cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo);

        // TRANSFORMENGINE: create — delegates to IFpsCostCentreApiClient.CreateCostCentreAsync
        /// <summary>
        /// Creates a new cost centre record.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto);

        // TRANSFORMENGINE: update — costCentreNo identifies the existing record; dto may carry updated field values
        /// <summary>
        /// Updates the cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto);

        // TRANSFORMENGINE: delete — delegates to IFpsCostCentreApiClient.DeleteCostCentreAsync
        /// <summary>
        /// Deletes the cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo);
    }
}
