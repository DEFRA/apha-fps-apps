namespace Apha.FPSApps.Application.Dtos.PACT;

public class SummarisedWgTimeViewDto
{
    public List<int> Months { get; set; } = [];
    public List<SummarisedWgTimeDto> Rows { get; set; } = [];
    public SummarisedWgTimeSummaryDto Summary { get; set; } = new();
    public PaginationDto Pagination { get; set; } = new();
    public List<SummarisedWgTimeProjectTitleLookupItem> ProjectTitleLookup { get; set; } = [];
}

public class SummarisedWgTimeProjectTitleLookupItem
{
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
}

public class SummarisedWgTimeSummaryDto
{
    public double TotalApril { get; set; }
    public double TotalMay { get; set; }
    public double TotalJune { get; set; }
    public double TotalJuly { get; set; }
    public double TotalAugust { get; set; }
    public double TotalSeptember { get; set; }
    public double TotalOctober { get; set; }
    public double TotalNovember { get; set; }
    public double TotalDecember { get; set; }
    public double TotalJanuary { get; set; }
    public double TotalFebruary { get; set; }
    public double TotalMarch { get; set; }
    public double GrandTotalTime { get; set; }
    public double GrandTotalCost { get; set; }
}