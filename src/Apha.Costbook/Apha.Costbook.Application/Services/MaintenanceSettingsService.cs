/*
 * TRANSFORMENGINE MIGRATION — MaintenanceSettingsService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New service implementation created for bulk maintenance settings (Tabs 1 and 4 of frmMaintainance)
 *   - GetSettingsAsync: fetches all Userupdateable=true rows from tbl_settings via ISettingsRepository,
 *     then maps named rows to MaintenanceSettingsDto by Settings.Id key
 *   - UpdateSettingsAsync: builds Dictionary<string, string> from MaintenanceSettingsDto fields and
 *     delegates to ISettingsRepository.UpdateMultipleAsync for bulk update
 *   - Settings IDs (from VBA _Constants.bas / mdlCostbook.bas constants):
 *       InflationAnimals, InflationExceptional, InflationStaff, InflationTests
 *       ProfitAnimals, ProfitExceptional, ProfitStaff, ProfitTests
 *       CurrentYear, WorkingHoursInDay, WorkingDaysInYear
 *   - ISettingsService interface extended with GetAllUserUpdatableAsync + UpdateMultipleAsync
 *     per the Interface changes log (PENDING → DONE via this service and IMaintenanceSettingsService)
 *
 * PRESERVED:
 *   - All VBA settings ID strings preserved verbatim (InflationExceptional not InflationExceptionalCosts)
 *   - Existing GetSettingValueByIdAsync path left untouched on ISettingsService/SettingsService
 *   - No direct DbContext usage — repository-only orchestration
 *   - InvalidOperationException thrown when required setting row is absent (mirrors VBA error handling)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal/string round-trip precision for tbl_settings.setting column (varchar 255)
 *   - TRANSFORMENGINE TODO: VBA fnInflation() multiplier formula — verify whether it is applied by caller or stored verbatim
 *   - TRANSFORMENGINE TODO: Validate CurrentFinancialYear range constraints match VBA guards (e.g. > 2000)
 */

using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;

namespace Apha.Costbook.Application.Services
{
    // TRANSFORMENGINE: Service implementation for IMaintenanceSettingsService — covers Tab 1 (Inflation) + Tab 4 (Profit Margins)
    public class MaintenanceSettingsService : IMaintenanceSettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        // TRANSFORMENGINE: VBA settings ID constants — must match mabarchive.tbl_settings id column verbatim
        private const string IdInflationAnimals       = "InflationAnimals";
        private const string IdInflationExceptional   = "InflationExceptional";
        private const string IdInflationStaff         = "InflationStaff";
        private const string IdInflationTests         = "InflationTests";
        private const string IdCurrentYear            = "CurrentYear";
        private const string IdWorkingHoursInDay      = "WorkingHoursInDay";
        private const string IdWorkingDaysInYear      = "WorkingDaysInYear";
        private const string IdProfitAnimals          = "ProfitAnimals";
        private const string IdProfitExceptional      = "ProfitExceptional";
        private const string IdProfitStaff            = "ProfitStaff";
        private const string IdProfitTests            = "ProfitTests";

        public MaintenanceSettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        // TRANSFORMENGINE: GetSettingsAsync — fetches all Userupdateable rows and maps each named row to DTO fields
        public async Task<MaintenanceSettingsDto> GetSettingsAsync()
        {
            var settings = await _settingsRepository.GetAllUserUpdatableAsync();

            // TRANSFORMENGINE: Build lookup by Id for O(1) access when mapping to DTO fields
            var lookup = settings.ToDictionary(s => s.Id, s => s.Setting ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            return new MaintenanceSettingsDto
            {
                // TRANSFORMENGINE: Inflation section — parse each setting row value to decimal
                InflationAnimals       = ParseDecimal(lookup, IdInflationAnimals),
                InflationExceptionalCosts = ParseDecimal(lookup, IdInflationExceptional),
                InflationStaff         = ParseDecimal(lookup, IdInflationStaff),
                InflationTests         = ParseDecimal(lookup, IdInflationTests),

                // TRANSFORMENGINE: Other Figures section
                CurrentFinancialYear   = ParseInt(lookup, IdCurrentYear),
                WorkingHoursInDay      = ParseDecimal(lookup, IdWorkingHoursInDay),
                WorkingDaysInYear      = ParseDecimal(lookup, IdWorkingDaysInYear),

                // TRANSFORMENGINE: Profit Margins section
                ProfitAnimals          = ParseDecimal(lookup, IdProfitAnimals),
                ProfitExceptionalCosts = ParseDecimal(lookup, IdProfitExceptional),
                ProfitStaff            = ParseDecimal(lookup, IdProfitStaff),
                ProfitTests            = ParseDecimal(lookup, IdProfitTests),
            };
        }

        // TRANSFORMENGINE: UpdateSettingsAsync — builds bulk update dictionary from DTO and delegates to repository
        public async Task UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            if (dto is null)
                throw new ArgumentException("MaintenanceSettingsDto must not be null.", nameof(dto));

            // TRANSFORMENGINE: Build settings dictionary; keys must match tbl_settings.id verbatim
            var updates = new Dictionary<string, string>
            {
                [IdInflationAnimals]     = dto.InflationAnimals.ToString("G"),
                [IdInflationExceptional] = dto.InflationExceptionalCosts.ToString("G"),
                [IdInflationStaff]       = dto.InflationStaff.ToString("G"),
                [IdInflationTests]       = dto.InflationTests.ToString("G"),
                [IdCurrentYear]          = dto.CurrentFinancialYear.ToString(),
                [IdWorkingHoursInDay]    = dto.WorkingHoursInDay.ToString("G"),
                [IdWorkingDaysInYear]    = dto.WorkingDaysInYear.ToString("G"),
                [IdProfitAnimals]        = dto.ProfitAnimals.ToString("G"),
                [IdProfitExceptional]    = dto.ProfitExceptionalCosts.ToString("G"),
                [IdProfitStaff]          = dto.ProfitStaff.ToString("G"),
                [IdProfitTests]          = dto.ProfitTests.ToString("G"),
            };

            // TRANSFORMENGINE: Delegate bulk update to repository; throws InvalidOperationException if no rows updated
            var success = await _settingsRepository.UpdateMultipleAsync(updates);
            if (!success)
                throw new InvalidOperationException("Maintenance settings update failed — no rows were updated in tbl_settings.");
        }

        // TRANSFORMENGINE: Private helper — parses a string setting row value to decimal; throws if key missing or parse fails
        private static decimal ParseDecimal(Dictionary<string, string> lookup, string id)
        {
            if (!lookup.TryGetValue(id, out var raw))
                throw new InvalidOperationException($"Required setting '{id}' was not found in tbl_settings (Userupdateable rows).");

            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out var value))
                throw new InvalidOperationException($"Setting '{id}' value '{raw}' could not be parsed as a decimal.");

            return value;
        }

        // TRANSFORMENGINE: Private helper — parses a string setting row value to int; throws if key missing or parse fails
        private static int ParseInt(Dictionary<string, string> lookup, string id)
        {
            if (!lookup.TryGetValue(id, out var raw))
                throw new InvalidOperationException($"Required setting '{id}' was not found in tbl_settings (Userupdateable rows).");

            if (!int.TryParse(raw, out var value))
                throw new InvalidOperationException($"Setting '{id}' value '{raw}' could not be parsed as an integer.");

            return value;
        }
    }
}
