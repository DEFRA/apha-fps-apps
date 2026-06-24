/*
 * TRANSFORMENGINE MIGRATION — IMaintenanceSettingsService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service interface created for bulk maintenance settings (Tabs 1 and 4 of frmMaintainance)
 *   - GetSettingsAsync — returns all user-updatable settings as a flat MaintenanceSettingsDto
 *   - UpdateSettingsAsync — applies bulk update from MaintenanceSettingsDto to tbl_settings rows
 *   - Maps to backend routes: GET /api/v1/Maintenance/settings, PUT /api/v1/Maintenance/settings
 *   - Settings IDs used: InflationAnimals, InflationExceptional, InflationStaff, InflationTests,
 *     CurrentYear, WorkingHoursInDay, WorkingDaysInYear, ProfitAnimals, ProfitExceptional,
 *     ProfitStaff, ProfitTests (from VBA _Constants.bas / mdlCostbook.bas)
 *
 * PRESERVED:
 *   - Async-only signatures per Application layer convention
 *   - No infrastructure-specific types (no DbContext, EF references)
 *   - ISettingsRepository interface changes log items PENDING → implemented by MaintenanceSettingsService
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm VBA fnInflation() multiplier formula is applied server-side, not just stored
 *   - TRANSFORMENGINE TODO: Validate CurrentFinancialYear range constraints
 */

using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for mabarchive.tbl_settings bulk operations — covers Tab 1 (Inflation) + Tab 4 (Profit Margins)
    public interface IMaintenanceSettingsService
    {
        /// <summary>
        /// Returns all user-updatable settings as a flat DTO.
        /// Maps to GET /api/v1/Maintenance/settings.
        /// Throws <see cref="InvalidOperationException"/> if required settings rows are missing from tbl_settings.
        /// </summary>
        Task<MaintenanceSettingsDto> GetSettingsAsync();

        /// <summary>
        /// Applies a bulk update of all user-updatable settings.
        /// Maps to PUT /api/v1/Maintenance/settings.
        /// Throws <see cref="ArgumentException"/> if dto is null.
        /// Throws <see cref="InvalidOperationException"/> if the update fails (e.g. no rows matched).
        /// </summary>
        Task UpdateSettingsAsync(MaintenanceSettingsDto dto);
    }
}
