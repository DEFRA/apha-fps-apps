/*
 * TRANSFORMENGINE MIGRATION — CostBookMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend service implementation created for frmMaintainance Tabs 1, 2, and 4
 *   - Implements ICostBookMaintenanceService as a thin delegate to ICostBookApiClient.CostbookMaintenance
 *   - GetSettingsAsync()              → _costBookClient.CostbookMaintenance.GetSettingsAsync()
 *   - UpdateSettingsAsync()           → _costBookClient.CostbookMaintenance.UpdateSettingsAsync()
 *   - GetAccountCategoriesAsync()     → _costBookClient.CostbookMaintenance.GetAccountCategoriesAsync()
 *   - UpdateAccountCategoryAsync()    → _costBookClient.CostbookMaintenance.UpdateAccountCategoryAsync()
 *   - _costBookClient is private readonly (Sonar S2933 compliance)
 *
 * PRESERVED:
 *   - No business logic — all methods are single-line return await delegates (Sonar S4144 intentional)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether a paginated overload is needed for account-categories (currently no pagination on backend)
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear filter parameter is needed on GetAccountCategoriesAsync (currently server-side derived)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    // TRANSFORMENGINE: Thin delegate service for frmMaintainance Tabs 1, 2, 4 — forwards to ICostBookApiClient.CostbookMaintenance
    public class CostBookMaintenanceService : ICostBookMaintenanceService
    {
        // TRANSFORMENGINE: private readonly — Sonar S2933 compliance
        private readonly ICostBookApiClient _costBookClient;

        public CostBookMaintenanceService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/maintenance/settings
        public Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync()
        {
            return _costBookClient.CostbookMaintenance.GetSettingsAsync();
        }

        // TRANSFORMENGINE: delegate → PUT /api/v1/maintenance/settings
        public Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            return _costBookClient.CostbookMaintenance.UpdateSettingsAsync(dto);
        }

        // TRANSFORMENGINE: delegate → GET /api/v1/maintenance/account-categories
        public Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync()
        {
            return _costBookClient.CostbookMaintenance.GetAccountCategoriesAsync();
        }

        // TRANSFORMENGINE: delegate → PUT /api/v1/maintenance/account-categories/{accShortName}
        public Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto)
        {
            return _costBookClient.CostbookMaintenance.UpdateAccountCategoryAsync(accShortName, dto);
        }
    }
}
