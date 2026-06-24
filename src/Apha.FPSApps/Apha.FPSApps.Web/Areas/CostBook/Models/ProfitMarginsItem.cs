/*
 * TRANSFORMENGINE MIGRATION — ProfitMarginsItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend Item model created for frmMaintainance Tab 4 (Profit Margins)
 *   - Properties derived from HTML prototype form fields: profitAnimals, profitExceptionalCosts,
 *     profitStaff, profitTests
 *   - Property names match MaintenanceSettingsDto exactly (profit sub-set)
 *   - Tab 4 is a static form (not a DataGrid) — no GridColumn attributes
 *   - All fields are numeric and required (per JS bindStaticFormValidation / formProfitMargins)
 *
 * PRESERVED:
 *   - All 4 profit margin fields from HTML prototype formProfitMargins
 *   - Required validation matches JS isNumericValue guard
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm fnProfit() formula (1 + p/(1-p)) rounding precision requirements
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: Item model for frmMaintainance Tab 4 (Profit Margins)
//   Maps to MaintenanceSettingsDto profit sub-set (ProfitAnimals, ProfitExceptionalCosts,
//   ProfitStaff, ProfitTests)
public class ProfitMarginsItem
{
    // TRANSFORMENGINE: HTML id=profitAnimals → MaintenanceSettingsDto.ProfitAnimals
    [Required(ErrorMessage = "Animals profit margin is required.")]
    [Display(Name = "Animals Profit (%)")]
    public decimal ProfitAnimals { get; set; }

    // TRANSFORMENGINE: HTML id=profitExceptionalCosts → MaintenanceSettingsDto.ProfitExceptionalCosts
    [Required(ErrorMessage = "Exceptional Costs profit margin is required.")]
    [Display(Name = "Exceptional Costs Profit (%)")]
    public decimal ProfitExceptionalCosts { get; set; }

    // TRANSFORMENGINE: HTML id=profitStaff → MaintenanceSettingsDto.ProfitStaff
    [Required(ErrorMessage = "Staff profit margin is required.")]
    [Display(Name = "Staff Profit (%)")]
    public decimal ProfitStaff { get; set; }

    // TRANSFORMENGINE: HTML id=profitTests → MaintenanceSettingsDto.ProfitTests
    [Required(ErrorMessage = "Tests profit margin is required.")]
    [Display(Name = "Tests Profit (%)")]
    public decimal ProfitTests { get; set; }
}
