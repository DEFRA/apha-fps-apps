using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class YearlyDetailsViewModel
{
    // ── Header (read-only) ─────────────────────────────────────────────────
    public string Code { get; set; } = string.Empty;
    public string? ProjectTitle { get; set; }
    public DateOnly? StartDate { get; set; }
    public int? FinancialYears { get; set; }
    public int? Inflation { get; set; }
    public short? IsDefraProject { get; set; }
    public double? EuroConvRate { get; set; }
    public string? Programme { get; set; }

    // ── Year navigation ────────────────────────────────────────────────────
    public int SelectedYear { get; set; }
    public List<int> ProjectYears { get; set; } = new();

    // ── Year totals (read-only, computed from subgrid footers) ─────────────
    public double StaffCostTotal { get; set; }
    public double TestCostTotal { get; set; }
    public double AnimalCostTotal { get; set; }
    public double AdditionalCostTotal { get; set; }
    public double GrandTotal => StaffCostTotal + TestCostTotal + AnimalCostTotal + AdditionalCostTotal;

    // ── DataGrid configs ───────────────────────────────────────────────────
    public DataGridConfig<StaffRequirementItem> StaffGrid { get; set; } = new();
    public DataGridConfig<TestRequirementItem> TestGrid { get; set; } = new();
    public DataGridConfig<AnimalRequirementItem> AnimalGrid { get; set; } = new();
    public DataGridConfig<AdditionalCostItem> AdditionalCostGrid { get; set; } = new();

    // ── Markup/Profit table (sf_ProjectYearRates — all years, editable) ────
    public List<ProjectYearRateItem> YearRates { get; set; } = new();

    // ── Dropdowns for modal forms ──────────────────────────────────────────
    public List<SelectListItem> WgGradeOptions { get; set; } = new();
    public List<SelectListItem> TestCodeOptions { get; set; } = new();
    public List<SelectListItem> AnimalTypeOptions { get; set; } = new();
    public List<SelectListItem> AccountCatOptions { get; set; } = new();
}
