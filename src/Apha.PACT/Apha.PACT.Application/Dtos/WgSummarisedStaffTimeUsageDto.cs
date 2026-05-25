using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos
{
    /// <summary>
    /// Represents one row in the Work Group Time By Job Code view for a person / job-code combination,
    /// with hours recorded against each of the 12 fiscal-year months (April – March).
    /// Mirrors the data shown in the legacy MS-Access form frmCluedo1.
    /// </summary>
    public class WgSummarisedStaffTimeUsageRowDto
    {
        public string? ParentProject { get; set; }
        public string? JobCode { get; set; }
        public string? JobTitle { get; set; }

        // Monthly hours — fiscal year runs April (month 1) to March (month 12)
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
    /// Wrapper returned by the service containing the pivot rows and pre-computed footer summary.
    /// </summary>
    public class WgSummarisedStaffTimeUsageDto
    {
        public IEnumerable<WgSummarisedStaffTimeUsageRowDto> Rows    { get; set; } = [];
        public WgSummarisedStaffTimeUsageSummaryDto          Summary { get; set; } = new();
        public PaginationDto Pagination { get; set; } = new();

        /// <summary>Total HrsPaid
        public double HrsPaid { get; set; }
    }
}
