/*
 * TRANSFORMENGINE MIGRATION — MaintenanceSettingsRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from MS Access frmMaintainance HTML prototype (Tab 1 + Tab 4)
 *   - Full RecordSource surface of mabarchive.tbl_settings for user-updatable rows
 *   - Covers all fields required by GET /api/v1/Maintenance/settings to populate both form tabs
 *
 * PRESERVED:
 *   - All field names aligned with VBA Settings IDs for round-trip fidelity
 *   - No EF entity or repository concerns leaked into contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Notes/TestSetting columns from tbl_settings are required for display
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Res contract for GET /api/v1/Maintenance/settings — populates Tab 1 (Inflation) and Tab 4 (Profit Margins)
    public class MaintenanceSettingsRes
    {
        // TRANSFORMENGINE: Inflation Figures section

        /// <summary>% Inflation figure for Animals.</summary>
        public decimal InflationAnimals { get; set; }

        /// <summary>% Inflation figure for Exceptional Costs.</summary>
        public decimal InflationExceptionalCosts { get; set; }

        /// <summary>% Inflation figure for Staff.</summary>
        public decimal InflationStaff { get; set; }

        /// <summary>% Inflation figure for Tests.</summary>
        public decimal InflationTests { get; set; }

        // TRANSFORMENGINE: Other Figures section

        /// <summary>Current Financial Year. Links lookup tables to the relevant FPS version.</summary>
        public int CurrentFinancialYear { get; set; }

        /// <summary>Working hours in a standard working day.</summary>
        public decimal WorkingHoursInDay { get; set; }

        /// <summary>Working days in a standard working year.</summary>
        public decimal WorkingDaysInYear { get; set; }

        // TRANSFORMENGINE: Profit Figures section (Tab 4 Profit Margins) — commercial projects only

        /// <summary>% Profit figure for Animals.</summary>
        public decimal ProfitAnimals { get; set; }

        /// <summary>% Profit figure for Exceptional Costs.</summary>
        public decimal ProfitExceptionalCosts { get; set; }

        /// <summary>% Profit figure for Staff.</summary>
        public decimal ProfitStaff { get; set; }

        /// <summary>% Profit figure for Tests.</summary>
        public decimal ProfitTests { get; set; }
    }
}
