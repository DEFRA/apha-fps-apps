using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;


public class MaintenanceViewModel
{
    
    [Display(Name = "Animals Inflation (%)")]
    public decimal InflationAnimals { get; set; }

    
    [Display(Name = "Exceptional Costs Inflation (%)")]
    public decimal InflationExceptionalCosts { get; set; }

    
    [Display(Name = "Staff Inflation (%)")]
    public decimal InflationStaff { get; set; }
    [Display(Name = "Tests Inflation (%)")]
    public decimal InflationTests { get; set; }
   
    [Display(Name = "Current Financial Year")]
    public int CurrentFinancialYear { get; set; }

    [Display(Name = "Working Hours in Day")]
    public decimal WorkingHoursInDay { get; set; }

    [Display(Name = "Working Days in Year")]
    public decimal WorkingDaysInYear { get; set; }
   
    public DataGridConfig<AccountCategoryItem> AccountCategoryGrid { get; set; } = new();
    public List<SelectListItem> Csg7GroupList { get; set; } = new();

    // ── Tab 3: CSG7 Inflation Options DataGrid ────────────────────────────────
    public DataGridConfig<Csg7GroupItem> Csg7GroupGrid { get; set; } = new();

    // ── Tab 4: Profit Margins ─────────────────────────────────────────────────
    
    [Display(Name = "Animals Profit (%)")]
    public decimal ProfitAnimals { get; set; }

    [Display(Name = "Exceptional Costs Profit (%)")]
    public decimal ProfitExceptionalCosts { get; set; }

    [Display(Name = "Staff Profit (%)")]
    public decimal ProfitStaff { get; set; }

    [Display(Name = "Tests Profit (%)")]
    public decimal ProfitTests { get; set; }

    // ── Tab 5: CAPS Staff DataGrid ────────────────────────────────────────────
    public DataGridConfig<CapsStaffItem> CapsStaffGrid { get; set; } = new();
}
