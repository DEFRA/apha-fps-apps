/*
 * TRANSFORMENGINE MIGRATION — ICostBookMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend service interface created for frmMaintainance Tabs 1, 2, and 4
 *   - Mirrors ICostBookMaintenanceApiClient signatures exactly (thin delegate pattern)
 *   - GetSettingsAsync()                  → delegates to CostbookMaintenance.GetSettingsAsync()
 *   - UpdateSettingsAsync()               → delegates to CostbookMaintenance.UpdateSettingsAsync()
 *   - GetAccountCategoriesAsync()         → delegates to CostbookMaintenance.GetAccountCategoriesAsync()
 *   - UpdateAccountCategoryAsync()        → delegates to CostbookMaintenance.UpdateAccountCategoryAsync()
 *
 * PRESERVED:
 *   - All return types and parameter signatures match ICostBookMaintenanceApiClient exactly
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether a paginated overload is needed for account-categories (currently no pagination on backend)
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear filter parameter is needed on GetAccountCategoriesAsync (currently server-side derived)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{
    // TRANSFORMENGINE: Service interface for frmMaintainance Tabs 1, 2, 4 — thin delegate pattern, mirrors ICostBookMaintenanceApiClient
    public interface ICostBookMaintenanceService
    {
        // TRANSFORMENGINE: GET /api/v1/maintenance/settings → returns all user-updatable maintenance settings (inflation + profit + system)
        /// <summary>Returns all user-updatable maintenance settings (inflation rates, working hours/days, profit margins).</summary>
        Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync();

        // TRANSFORMENGINE: PUT /api/v1/maintenance/settings → bulk update all maintenance settings; Admin role required
        /// <summary>Applies a bulk update of all user-updatable maintenance settings.</summary>
        Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto);

        // TRANSFORMENGINE: GET /api/v1/maintenance/account-categories → returns all account categories for Tab 2 grid
        /// <summary>Returns all account categories for the maintenance grid (Tab 2 of frmMaintainance).</summary>
        Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync();

        // TRANSFORMENGINE: PUT /api/v1/maintenance/account-categories/{accShortName} → update CSG7 group on a single account category
        /// <summary>Updates the CSG7 group assignment on an existing account category.</summary>
        Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto);
    }
}
