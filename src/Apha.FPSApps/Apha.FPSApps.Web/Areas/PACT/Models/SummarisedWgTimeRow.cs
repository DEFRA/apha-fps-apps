namespace Apha.FPSApps.Web.Areas.PACT.Models;

public class SummarisedWgTimeRow
{
    public string WorkGroup { get; set; } = string.Empty;
    public string? ProfitCentre { get; set; }
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;

    // Monthly time allocations (M1 = April, M2 = May, ..., M12 = March)
    public decimal M1 { get; set; }   // April
    public decimal M2 { get; set; }   // May
    public decimal M3 { get; set; }   // June
    public decimal M4 { get; set; }   // July
    public decimal M5 { get; set; }   // August
    public decimal M6 { get; set; }   // September
    public decimal M7 { get; set; }   // October
    public decimal M8 { get; set; }   // November
    public decimal M9 { get; set; }   // December
    public decimal M10 { get; set; }  // January
    public decimal M11 { get; set; }  // February
    public decimal M12 { get; set; }  // March

    // Computed totals
    public decimal SumOfTime { get; set; }
    public decimal SumOfCost { get; set; }
    public decimal? Budget { get; set; }
    public decimal? PercentSpent { get; set; }
}
