/*
 * TRANSFORMENGINE MIGRATION — ICostBookMaintenanceApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend API client interface created for frmMaintainance Tabs 1, 2, and 4
 *   - Targets backend MaintenanceController at route /api/v1/maintenance
 *   - GetSettingsAsync()            → GET  /api/v1/maintenance/settings
 *   - UpdateSettingsAsync()         → PUT  /api/v1/maintenance/settings
 *   - GetAccountCategoriesAsync()   → GET  /api/v1/maintenance/account-categories
 *   - UpdateAccountCategoryAsync()  → PUT  /api/v1/maintenance/account-categories/{accShortName}
 *   - All return types wrapped in ApiResponseDto<T>
 *
 * PRESERVED:
 *   - Backend endpoint semantics and route parameters preserved exactly
 *   - AccountCategoryMaintenanceDto used for both list and single-item update responses
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether a paginated overload is needed for account-categories (currently no pagination on backend)
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear filter parameter is needed on GetAccountCategoriesAsync (currently server-side derived)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

// TRANSFORMENGINE: API client interface for backend MaintenanceController — covers frmMaintainance Tabs 1, 2, 4
//   Tab 1 (Inflation Figures) + Tab 4 (Profit Margins) → settings GET/PUT
//   Tab 2 (Account Categories)                         → account-categories GET/PUT
public interface ICostBookMaintenanceApiClient
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

    // TRANSFORMENGINE: PUT /api/v1/maintenance/account-categories/{accShortName} → update CSG7 group on a single account category; Admin role required
    /// <summary>Updates the CSG7 group assignment on an existing account category.</summary>
    Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto);
}
