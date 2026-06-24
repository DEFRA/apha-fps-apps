/*
 * TRANSFORMENGINE MIGRATION — MaintenanceSettingsDto.cs (Frontend)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend DTO created in Apha.FPSApps.Application, mirroring backend Apha.Costbook.Application.Dtos.MaintenanceSettingsDto
 *   - Same shape as backend DTO — 11 properties covering Tab 1 (Inflation Figures) + Tab 4 (Profit Margins) + system settings
 *   - Used by ICostBookMaintenanceApiClient and frontend CostBookMaintenanceService to deserialise API responses
 *   - Namespace: Apha.FPSApps.Application.Dtos.CostBook (frontend application layer)
 *
 * PRESERVED:
 *   - All property names exactly match backend DTO (case-sensitive): InflationAnimals, InflationExceptionalCosts,
 *     InflationStaff, InflationTests, CurrentFinancialYear, WorkingHoursInDay, WorkingDaysInYear,
 *     ProfitAnimals, ProfitExceptionalCosts, ProfitStaff, ProfitTests
 *   - All property types match backend (decimal / int)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal precision requirements align with backend fnInflation() and fnProfit() rounding behaviour
 */

namespace Apha.FPSApps.Application.Dtos.CostBook;

// TRANSFORMENGINE: Frontend mirror of Apha.Costbook.Application.Dtos.MaintenanceSettingsDto
//   Covers frmMaintainance Tab 1 (Inflation Figures) and Tab 4 (Profit Margins) settings
public class MaintenanceSettingsDto
{
    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.InflationAnimals
    /// <summary>% Inflation figure for Animals (e.g. 2.5 means 2.5%).</summary>
    public decimal InflationAnimals { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.InflationExceptionalCosts
    /// <summary>% Inflation figure for Exceptional Costs.</summary>
    public decimal InflationExceptionalCosts { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.InflationStaff
    /// <summary>% Inflation figure for Staff.</summary>
    public decimal InflationStaff { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.InflationTests
    /// <summary>% Inflation figure for Tests.</summary>
    public decimal InflationTests { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.CurrentFinancialYear
    /// <summary>Current Financial Year. Links lookup tables to the relevant FPS version.</summary>
    public int CurrentFinancialYear { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.WorkingHoursInDay
    /// <summary>Working hours in a standard working day (e.g. 7.2).</summary>
    public decimal WorkingHoursInDay { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.WorkingDaysInYear
    /// <summary>Working days in a standard working year (e.g. 220.5).</summary>
    public decimal WorkingDaysInYear { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.ProfitAnimals
    /// <summary>% Profit figure for Animals (e.g. 15.00 means 15%).</summary>
    public decimal ProfitAnimals { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.ProfitExceptionalCosts
    /// <summary>% Profit figure for Exceptional Costs.</summary>
    public decimal ProfitExceptionalCosts { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.ProfitStaff
    /// <summary>% Profit figure for Staff.</summary>
    public decimal ProfitStaff { get; set; }

    // TRANSFORMENGINE: Maps to backend MaintenanceSettingsDto.ProfitTests
    /// <summary>% Profit figure for Tests.</summary>
    public decimal ProfitTests { get; set; }
}
