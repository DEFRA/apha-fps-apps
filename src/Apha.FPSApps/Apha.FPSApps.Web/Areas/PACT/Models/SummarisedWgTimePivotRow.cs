namespace Apha.FPSApps.Web.Areas.PACT.Models;

public class SummarisedWgTimePivotRow
{
    public string WorkGroup { get; set; } = string.Empty;
    public string? ProfitCentre { get; set; }
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;

    // Monthly time values (M1 = April through M12 = March)
    public decimal? M1 { get; set; }
    public decimal? M2 { get; set; }
    public decimal? M3 { get; set; }
    public decimal? M4 { get; set; }
    public decimal? M5 { get; set; }
    public decimal? M6 { get; set; }
    public decimal? M7 { get; set; }
    public decimal? M8 { get; set; }
    public decimal? M9 { get; set; }
    public decimal? M10 { get; set; }
    public decimal? M11 { get; set; }
    public decimal? M12 { get; set; }

    // Computed totals
    public decimal SumOfTime { get; set; }
    public decimal SumOfCost { get; set; }
    public decimal? Budget { get; set; }
    public decimal? PercentSpent { get; set; }
}
