using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class ProjectSummaryRow
{
    [GridColumn(Order = 0, IsVisible = false)]
    public int Year { get; set; }

    [Display(Name = "Financial Year")]
    [GridColumn(Order = 1, Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = false)]
    public string FinancialYearDisplay => $"{Year}/{Year + 1}";

    [Display(Name = "Additional Costs (£)")]
    [GridColumn(Order = 2, Width = 140, Type = GridColumnType.GbpValue)]
    public double AdditionalCost { get; set; }

    [Display(Name = "Staff Costs (£)")]
    [GridColumn(Order = 3, Width = 140, Type = GridColumnType.GbpValue)]
    public double StaffCost { get; set; }

    [Display(Name = "Test Costs (£)")]
    [GridColumn(Order = 4, Width = 140, Type = GridColumnType.GbpValue)]
    public double TestCost { get; set; }

    [Display(Name = "Animal Costs (£)")]
    [GridColumn(Order = 5, Width = 140, Type = GridColumnType.GbpValue)]
    public double AnimalCost { get; set; }

    [Display(Name = "Total Year Costs (£)")]
    [GridColumn(Order = 6, Width = 150, Type = GridColumnType.GbpValue)]
    public double GrandTotal => StaffCost + TestCost + AnimalCost + AdditionalCost;

    [GridColumn(Order = 7, IsVisible = false)]
    public double ProfitIncludedTotal { get; set; }
}

public class ProjectSummaryViewModel
{
    public ProjectHeaderDto ProjectHeaderDto { get; set; } = new();
    public List<ProjectSummaryRow> Rows { get; set; } = new();
    public DataGridConfig<ProjectSummaryRow> SummaryGrid { get; set; } = new();

    // ── Column totals (aggregated across all years) ────────────────────────
    public double TotalStaffCost        => Rows.Sum(r => r.StaffCost);
    public double TotalTestCost         => Rows.Sum(r => r.TestCost);
    public double TotalAnimalCost       => Rows.Sum(r => r.AnimalCost);
    public double TotalAdditionalCost   => Rows.Sum(r => r.AdditionalCost);
    public double GrandTotal            => TotalStaffCost + TotalTestCost + TotalAnimalCost + TotalAdditionalCost;

    // ── Aliases matching reference cshtml property names ───────────────────
    public double TotalStaffCosts       => TotalStaffCost;
    public double TotalTestCosts        => TotalTestCost;
    public double TotalAnimalCosts      => TotalAnimalCost;
    public double TotalAdditionalCosts  => TotalAdditionalCost;
    public double TotalCosts            => GrandTotal;

    // ── Incl Profit ────────────────────────────────────────────────────────
    // Aggregated sum of fnProfit totals across all years (from repository)
    public double TotalProfitIncluded   => Rows.Sum(r => r.ProfitIncludedTotal);

    // Shown only when Programme == "Comm"; InclProfit is now driven by TotalProfitIncluded
    public bool   ShowInclProfit { get; set; }
    public double InclProfit     => TotalProfitIncluded;
}
