namespace Apha.FPSApps.Application.Dtos.PACT
{
    /// <summary>
    /// Represents one row in the Work Group Time By Job Code view for a person / job-code combination,
    /// with hours recorded against each of the 12 fiscal-year months (April – March).
    /// Mirrors the data shown in the legacy MS-Access form frmCluedo1.
    /// </summary>
    public class WorkGroupTimeByJobCodeRowDto
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

    /// <summary>Pre-computed footer totals passed from the PACT API.</summary>
    public class WorkGroupTimeByJobCodeSummaryDto
    {
        public double TotalApril            { get; set; }
        public double TotalMay              { get; set; }
        public double TotalJune             { get; set; }
        public double TotalJuly             { get; set; }
        public double TotalAugust           { get; set; }
        public double TotalSeptember        { get; set; }
        public double TotalOctober          { get; set; }
        public double TotalNovember         { get; set; }
        public double TotalDecember         { get; set; }
        public double TotalJanuary          { get; set; }
        public double TotalFebruary         { get; set; }
        public double TotalMarch            { get; set; }
        public double GrandTotalTime        { get; set; }
        public double GrandTotalCost        { get; set; }
        public double StandardHoursPerMonth { get; set; }
        public double TotalStandardHours { get; set; }
        public double GrandTotalPercentAllocated { get; set; }
        public double PercentAllocatedApril              { get; set; }
        public double PercentAllocatedMay               { get; set; }
        public double PercentAllocatedJune               { get; set; }
        public double PercentAllocatedJuly               { get; set; }
        public double PercentAllocatedAugust             { get; set; }
        public double PercentAllocatedSeptember          { get; set; }
        public double PercentAllocatedOctober            { get; set; }
        public double PercentAllocatedNovember           { get; set; }
        public double PercentAllocatedDecember           { get; set; }
        public double PercentAllocatedJanuary            { get; set; }
        public double PercentAllocatedFebruary           { get; set; }
        public double PercentAllocatedMarch              { get; set; }
    }

    /// <summary>Wrapper containing rows and pre-computed footer summary.</summary>
    public class WorkGroupTimeByJobCodeDto
    {
        public IEnumerable<WorkGroupTimeByJobCodeRowDto> Rows    { get; set; } = [];
        public WorkGroupTimeByJobCodeSummaryDto          Summary { get; set; } = new();
        public Apha.FPSApps.Application.Dtos.PaginationDto Pagination { get; set; } = new();
        public double                               HrsPaid    { get; set; }
    }
}
