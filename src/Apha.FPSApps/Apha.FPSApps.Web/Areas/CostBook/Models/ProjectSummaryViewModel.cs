using Apha.FPSApps.Application.Dtos.CostBook;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class ProjectSummaryRow
{
    public int Year { get; set; }
    public string FinancialYearDisplay => $"{Year}/{Year + 1}";
    
    public double StaffCost { get; set; }
    public double TestCost { get; set; }
    public double AnimalCost { get; set; }
    public double AdditionalCost { get; set; }
    
    public double GrandTotal => StaffCost + TestCost + AnimalCost + AdditionalCost;
    
    public double ProfitIncludedTotal { get; set; }}

public class ProjectSummaryViewModel
{
    public ProjectHeaderDto ProjectHeaderDto { get; set; } = new();
    public List<ProjectSummaryRow> Rows { get; set; } = new();

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
