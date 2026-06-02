using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos
{
    public class SummarisedWgTimeViewDto
    {
        public IEnumerable<SummarisedWgTimeRowDto> Rows { get; set; } = [];
        public SummarisedWgTimeSummaryDto Summary { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();
        public List<ProjectTitleLookupItem> ProjectTitleLookup { get; set; } = [];
    }

    public class SummarisedWgTimeEntryDto
    {
        public int? FpsYear { get; set; }
        public string? MonthName { get; set; }
        public string? ProfitCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? ParentProject { get; set; }
        public string? ProjectTitle { get; set; }
        public double? TotalTime { get; set; }
        public double? TotalCost { get; set; }
    }

    public class SummarisedWgTimeRowDto
    {
        public string? ParentProject { get; set; }
        public double April { get; set; }
        public double May { get; set; }
        public double June { get; set; }
        public double July { get; set; }
        public double August { get; set; }
        public double September { get; set; }
        public double October { get; set; }
        public double November { get; set; }
        public double December { get; set; }
        public double January { get; set; }
        public double February { get; set; }
        public double March { get; set; }
        public double TotalTime { get; set; }
        public double TotalCost { get; set; }
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

    public class ProjectTitleLookupItem
    {
        public string ParentProject { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
    }
}