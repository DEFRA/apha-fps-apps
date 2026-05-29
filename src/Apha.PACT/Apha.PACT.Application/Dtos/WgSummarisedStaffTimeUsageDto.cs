using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos
{
    /// <summary>
    /// Wrapper returned by the service containing the pivot rows and pre-computed footer summary.
    /// </summary>
    public class WgSummarisedStaffTimeUsageDto
    {
        public IEnumerable<WgSummarisedStaffTimeUsageRowDto> Rows { get; set; } = [];
        public WgSummarisedStaffTimeUsageSummaryDto Summary { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();

        public double HrsPaid { get; set; }

        /// <summary>
        /// Complete JobCode → JobTitle list built from all rows (pre-pagination)
        /// so clients can resolve job titles across any page without an extra round-trip.
        /// </summary>
        public List<JobTitleLookupItem> JobTitleLookup { get; set; } = [];
    }

    /// <summary>
    /// Pivot row representing total hours and cost per fiscal month for a single ParentProject and JobCode combination.
    /// </summary>
    public class WgSummarisedStaffTimeUsageRowDto
    {
        public string? ParentProject { get; set; }
        public string? JobCode { get; set; }
        public string? JobTitle { get; set; }

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

    /// <summary>
    /// Pre-computed footer totals for the Work Group Time By Job Code view,
    /// calculated once in the service layer.
    /// </summary>
    public class WgSummarisedStaffTimeUsageSummaryDto
    {
        // Row 1 — Total time recorded per month
        public double TotalApril     { get; set; }
        public double TotalMay       { get; set; }
        public double TotalJune      { get; set; }
        public double TotalJuly      { get; set; }
        public double TotalAugust    { get; set; }
        public double TotalSeptember { get; set; }
        public double TotalOctober   { get; set; }
        public double TotalNovember  { get; set; }
        public double TotalDecember  { get; set; }
        public double TotalJanuary   { get; set; }
        public double TotalFebruary  { get; set; }
        public double TotalMarch     { get; set; }
        public double GrandTotalTime { get; set; }
        public double GrandTotalCost { get; set; }

        // Row 2 — Standard hours per month: HrsPaid / 12
        public double StandardHoursPerMonth { get; set; }
        // Field66: sum of per-month standard hours (only months with data contribute)
        public double TotalStandardHours { get; set; }

        // Grand total % allocated: GrandTotalTime / (StandardHoursPerMonth * 12) * 100
        public double GrandTotalPercentAllocated { get; set; }

        // Row 3 — % of standard hours allocated per month  (TotalMonth / StandardHoursPerMonth * 100)
        public double PercentAllocatedApril     { get; set; }
        public double PercentAllocatedMay       { get; set; }
        public double PercentAllocatedJune      { get; set; }
        public double PercentAllocatedJuly      { get; set; }
        public double PercentAllocatedAugust    { get; set; }
        public double PercentAllocatedSeptember { get; set; }
        public double PercentAllocatedOctober   { get; set; }
        public double PercentAllocatedNovember  { get; set; }
        public double PercentAllocatedDecember  { get; set; }
        public double PercentAllocatedJanuary   { get; set; }
        public double PercentAllocatedFebruary  { get; set; }
        public double PercentAllocatedMarch     { get; set; }
    }

    /// <summary>
    /// Represents a single JobCode → JobTitle pair used in the client-side lookup.
    /// Avoids Dictionary serialisation quirks through AutoMapper mapping chains.
    /// </summary>
    public class JobTitleLookupItem
    {
        public string JobCode  { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
    }

    /// <summary>
    /// Flat DTO representing one raw row returned by the repository for the summarised
    /// staff time-usage query. Maps 1-to-1 from <c>WgSummarisedStaffTimeUsageView</c>
    /// so that the Application service layer is fully decoupled from the EF-mapped entity.
    /// </summary>
    public class WgSummarisedStaffTimeUsageEntryDto
    {
        public string? MonthName { get; set; }
        public string? Name { get; set; }
        public double? HrsPaid { get; set; }
        public string? ParentProject { get; set; }
        public string? JobCode { get; set; }
        public string? JobTitle { get; set; }
        public double? TotalTime { get; set; }
        public double? TotalCost { get; set; }
    }
}