/*
 * TRANSFORMENGINE MIGRATION — MaintenanceSettingsDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + Services
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New DTO created for bulk maintenance settings from mabarchive.tbl_settings
 *   - Covers Tab 1 (Inflation Figures) and Tab 4 (Profit Margins) of frmMaintainance
 *   - Settings IDs from VBA constants: InflationAnimals, InflationExceptional, InflationStaff,
 *     InflationTests, ProfitAnimals, ProfitExceptional, ProfitStaff, ProfitTests,
 *     CurrentYear, WorkingHoursInDay, WorkingDaysInYear
 *   - Service maps List<Settings> (Userupdateable=true rows) to this flat DTO
 *   - No AutoMapper entry needed — MaintenanceSettingsService builds this manually from settings rows
 *
 * PRESERVED:
 *   - All field names aligned with VBA Settings IDs for round-trip fidelity
 *   - Decimal precision retained (no rounding at DTO level)
 *   - Business logic for fnInflation() and fnProfit() NOT embedded here — preserved in service
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal precision requirements with VBA fnInflation() and fnProfit() logic
 *   - TRANSFORMENGINE TODO: Validate CurrentFinancialYear range constraints match VBA guards (e.g. > 2000)
 */

namespace Apha.Costbook.Application.Dtos
{
    // TRANSFORMENGINE: Service-layer DTO for mabarchive.tbl_settings (Userupdateable=true rows) — covers Tab 1 + Tab 4
    public class MaintenanceSettingsDto
    {
        // TRANSFORMENGINE: Maps to tbl_settings row id='InflationAnimals' setting column
        /// <summary>% Inflation figure for Animals (e.g. 2.5 means 2.5%).</summary>
        public decimal InflationAnimals { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='InflationExceptional' setting column
        /// <summary>% Inflation figure for Exceptional Costs.</summary>
        public decimal InflationExceptionalCosts { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='InflationStaff' setting column
        /// <summary>% Inflation figure for Staff.</summary>
        public decimal InflationStaff { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='InflationTests' setting column
        /// <summary>% Inflation figure for Tests.</summary>
        public decimal InflationTests { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='CurrentYear' setting column
        /// <summary>Current Financial Year. Links lookup tables to the relevant FPS version.</summary>
        public int CurrentFinancialYear { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='WorkingHoursInDay' setting column
        /// <summary>Working hours in a standard working day (e.g. 7.2).</summary>
        public decimal WorkingHoursInDay { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='WorkingDaysInYear' setting column
        /// <summary>Working days in a standard working year (e.g. 220.5).</summary>
        public decimal WorkingDaysInYear { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='ProfitAnimals' setting column
        /// <summary>% Profit figure for Animals (e.g. 15.00 means 15%).</summary>
        public decimal ProfitAnimals { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='ProfitExceptional' setting column
        /// <summary>% Profit figure for Exceptional Costs.</summary>
        public decimal ProfitExceptionalCosts { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='ProfitStaff' setting column
        /// <summary>% Profit figure for Staff.</summary>
        public decimal ProfitStaff { get; set; }

        // TRANSFORMENGINE: Maps to tbl_settings row id='ProfitTests' setting column
        /// <summary>% Profit figure for Tests.</summary>
        public decimal ProfitTests { get; set; }
    }
}
