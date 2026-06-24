/*
 * TRANSFORMENGINE MIGRATION — MaintenanceSettingsReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from MS Access frmMaintainance HTML prototype (Tab 1 Inflation + Tab 4 Profit Margins)
 *   - Inflation fields mapped from: inflAnimals, inflExceptionalCosts, inflStaff, inflTests
 *   - System settings fields mapped from: inflCurrentFinancialYear, inflWorkingHoursInDay, inflWorkingDaysInYear
 *   - Profit fields mapped from: profitAnimals, profitExceptionalCosts, profitStaff, profitTests
 *   - Field names aligned with settings IDs from VBA: InflationAnimals, InflationExceptional, InflationStaff,
 *     InflationTests, ProfitAnimals, ProfitExceptional, ProfitStaff, ProfitTests,
 *     CurrentYear, WorkingHoursInDay, WorkingDaysInYear (mabarchive.tbl_settings)
 *
 * PRESERVED:
 *   - Writable input fields only — no response-only or entity fields included
 *   - No business logic (inflation multiplier, profit formula) in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal precision requirements with VBA fnInflation() and fnProfit() logic
 *   - TRANSFORMENGINE TODO: Validate CurrentFinancialYear range constraints match VBA guards
 */

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Req contract for PUT /api/v1/Maintenance/settings — covers both Inflation (Tab 1) and Profit Margins (Tab 4)
    public class MaintenanceSettingsReq
    {
        // TRANSFORMENGINE: Inflation Figures section — maps to mabarchive.tbl_settings rows (userupdateable=true)

        /// <summary>% Inflation figure for Animals (e.g. 2.5 means 2.5%).</summary>
        public decimal InflationAnimals { get; set; }

        /// <summary>% Inflation figure for Exceptional Costs.</summary>
        public decimal InflationExceptionalCosts { get; set; }

        /// <summary>% Inflation figure for Staff.</summary>
        public decimal InflationStaff { get; set; }

        /// <summary>% Inflation figure for Tests.</summary>
        public decimal InflationTests { get; set; }

        // TRANSFORMENGINE: Other Figures section — system-wide settings stored in mabarchive.tbl_settings

        /// <summary>Current Financial Year (e.g. 2024). Links lookup tables to the relevant FPS version.</summary>
        public int CurrentFinancialYear { get; set; }

        /// <summary>Working hours in a standard working day (e.g. 7.2).</summary>
        public decimal WorkingHoursInDay { get; set; }

        /// <summary>Working days in a standard working year (e.g. 220.5).</summary>
        public decimal WorkingDaysInYear { get; set; }

        // TRANSFORMENGINE: Profit Figures section (Tab 4 Profit Margins) — applies to commercial programme projects only

        /// <summary>% Profit figure for Animals (e.g. 15.00 means 15%).</summary>
        public decimal ProfitAnimals { get; set; }

        /// <summary>% Profit figure for Exceptional Costs.</summary>
        public decimal ProfitExceptionalCosts { get; set; }

        /// <summary>% Profit figure for Staff.</summary>
        public decimal ProfitStaff { get; set; }

        /// <summary>% Profit figure for Tests.</summary>
        public decimal ProfitTests { get; set; }
    }
}
