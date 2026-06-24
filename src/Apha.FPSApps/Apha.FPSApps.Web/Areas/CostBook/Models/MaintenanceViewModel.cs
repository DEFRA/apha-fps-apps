/*
 * TRANSFORMENGINE MIGRATION — MaintenanceViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend ViewModel created for frmMaintainance (all 5 tabs)
 *   - Tab 1 (Inflation Figures): 7 scalar properties matching MaintenanceSettingsDto inflation sub-set
 *   - Tab 2 (Account Categories): DataGridConfig<AccountCategoryItem> for the accCatGrid
 *   - Tab 3 (CSG7 Inflation Options): DataGridConfig<Csg7GroupItem> for the csg7Grid
 *   - Tab 4 (Profit Margins): 4 scalar properties matching MaintenanceSettingsDto profit sub-set
 *   - Tab 5 (CAPS Staff): DataGridConfig<CapsStaffItem> for the capsStaffGrid
 *   - Csg7GroupList: SelectListItem dropdown for AccountCategory modal (modal-acccat-csg7group is a
 *     <select> element inside the tblAccCatModal — explicit dropdown source required)
 *   - DataGridConfig instances are left as new() here; controller builds them explicitly in Index()
 *
 * PRESERVED:
 *   - All property names match MaintenanceSettingsDto exactly (no aliasing)
 *   - Inflation and Profit properties kept as separate tab-scoped sets for form binding clarity
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal precision requirements align with fnInflation() / fnProfit()
 *   - TRANSFORMENGINE TODO: Confirm whether CurrentFinancialYear needs a year-picker SelectList
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: ViewModel for frmMaintainance — covers all 5 tabs
//   Scalar settings properties bind directly to MaintenanceSettingsDto (inflation + profit sub-sets)
//   Three DataGridConfig properties drive the three grid tabs (Tabs 2, 3, 5)
public class MaintenanceViewModel
{
    // ── Tab 1: Inflation Figures ──────────────────────────────────────────────

    // TRANSFORMENGINE: HTML id=inflAnimals → MaintenanceSettingsDto.InflationAnimals
    [Display(Name = "Animals Inflation (%)")]
    public decimal InflationAnimals { get; set; }

    // TRANSFORMENGINE: HTML id=inflExceptionalCosts → MaintenanceSettingsDto.InflationExceptionalCosts
    [Display(Name = "Exceptional Costs Inflation (%)")]
    public decimal InflationExceptionalCosts { get; set; }

    // TRANSFORMENGINE: HTML id=inflStaff → MaintenanceSettingsDto.InflationStaff
    [Display(Name = "Staff Inflation (%)")]
    public decimal InflationStaff { get; set; }

    // TRANSFORMENGINE: HTML id=inflTests → MaintenanceSettingsDto.InflationTests
    [Display(Name = "Tests Inflation (%)")]
    public decimal InflationTests { get; set; }

    // TRANSFORMENGINE: HTML id=inflCurrentFinancialYear → MaintenanceSettingsDto.CurrentFinancialYear
    [Display(Name = "Current Financial Year")]
    public int CurrentFinancialYear { get; set; }

    // TRANSFORMENGINE: HTML id=inflWorkingHoursInDay → MaintenanceSettingsDto.WorkingHoursInDay
    [Display(Name = "Working Hours in Day")]
    public decimal WorkingHoursInDay { get; set; }

    // TRANSFORMENGINE: HTML id=inflWorkingDaysInYear → MaintenanceSettingsDto.WorkingDaysInYear
    [Display(Name = "Working Days in Year")]
    public decimal WorkingDaysInYear { get; set; }

    // ── Tab 2: Account Categories DataGrid ───────────────────────────────────

    // TRANSFORMENGINE: DataGridConfig for accCatGrid (Tab 2 — Enter CSG7 Groups for Account Categories)
    //   Built explicitly in Index() — never left as default new()
    public DataGridConfig<AccountCategoryItem> AccountCategoryGrid { get; set; } = new();

    // TRANSFORMENGINE: Dropdown for modal-acccat-csg7group <select> (Tab 2 AccountCategory modal)
    //   Source: ICostBookAccountGroupService.GetAllAccountGroupsAsync() → AccountGroupDto.Csg7Group
    //   This is an explicit <select> element inside tblAccCatModal — correct page-level dropdown
    public List<SelectListItem> Csg7GroupList { get; set; } = new();

    // ── Tab 3: CSG7 Inflation Options DataGrid ────────────────────────────────

    // TRANSFORMENGINE: DataGridConfig for csg7Grid (Tab 3 — Set Inflation Option for CSG7 groups)
    //   Built explicitly in Index() — never left as default new()
    public DataGridConfig<Csg7GroupItem> Csg7GroupGrid { get; set; } = new();

    // ── Tab 4: Profit Margins ─────────────────────────────────────────────────

    // TRANSFORMENGINE: HTML id=profitAnimals → MaintenanceSettingsDto.ProfitAnimals
    [Display(Name = "Animals Profit (%)")]
    public decimal ProfitAnimals { get; set; }

    // TRANSFORMENGINE: HTML id=profitExceptionalCosts → MaintenanceSettingsDto.ProfitExceptionalCosts
    [Display(Name = "Exceptional Costs Profit (%)")]
    public decimal ProfitExceptionalCosts { get; set; }

    // TRANSFORMENGINE: HTML id=profitStaff → MaintenanceSettingsDto.ProfitStaff
    [Display(Name = "Staff Profit (%)")]
    public decimal ProfitStaff { get; set; }

    // TRANSFORMENGINE: HTML id=profitTests → MaintenanceSettingsDto.ProfitTests
    [Display(Name = "Tests Profit (%)")]
    public decimal ProfitTests { get; set; }

    // ── Tab 5: CAPS Staff DataGrid ────────────────────────────────────────────

    // TRANSFORMENGINE: DataGridConfig for capsStaffGrid (Tab 5 — CAPS Staff CRUD)
    //   Built explicitly in Index() — never left as default new()
    public DataGridConfig<CapsStaffItem> CapsStaffGrid { get; set; } = new();
}
