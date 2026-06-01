namespace Apha.FPSApps.Application.Dtos.PACT;

public class SummarisedWgTimeDto
{
    public string WorkGroup { get; set; } = string.Empty;
    public string? ProfitCentre { get; set; }
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;

    // Monthly time allocations (financial year: April = period 1, March = period 12)
    public decimal? April { get; set; }
    public decimal? May { get; set; }
    public decimal? June { get; set; }
    public decimal? July { get; set; }
    public decimal? August { get; set; }
    public decimal? September { get; set; }
    public decimal? October { get; set; }
    public decimal? November { get; set; }
    public decimal? December { get; set; }
    public decimal? January { get; set; }
    public decimal? February { get; set; }
    public decimal? March { get; set; }

    // Computed totals
    public decimal SumOfTime { get; set; }
    public decimal SumOfCost { get; set; }
    public decimal? Budget { get; set; }
    public decimal? PercentSpent { get; set; }
}
