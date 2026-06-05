namespace Apha.Common.Contracts.PACT;

public class SummarisedWgTimePivotRes
{
    public List<int> Months { get; set; } = [];
    public List<SummarisedWgTimeRes> Rows { get; set; } = [];
    public SummarisedWgTimeSummaryRes Summary { get; set; } = new();
    public Pagination Pagination { get; set; } = new();
    public List<ProjectTitleLookupRes> ProjectTitleLookup { get; set; } = [];
}

public class ProjectTitleLookupRes
{
    public string ParentProject { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
}

public class SummarisedWgTimeSummaryRes
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