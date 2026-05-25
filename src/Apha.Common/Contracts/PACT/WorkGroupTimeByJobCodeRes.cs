namespace Apha.Common.Contracts.PACT
{
    /// <summary>
    /// API response contract for a single row in the Work Group Time By Job Code view,
    /// representing one person / job-code combination with hours per fiscal month.
    /// </summary>
    public class WorkGroupTimeByJobCodeRowRes
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

    /// <summary>Pre-computed footer totals returned alongside the row data.</summary>
    public class WorkGroupTimeByJobCodeSummaryRes
    {
        public double TotalApril          { get; set; }
        public double TotalMay            { get; set; }
        public double TotalJune           { get; set; }
        public double TotalJuly           { get; set; }
        public double TotalAugust         { get; set; }
        public double TotalSeptember      { get; set; }
        public double TotalOctober        { get; set; }
        public double TotalNovember       { get; set; }
        public double TotalDecember       { get; set; }
        public double TotalJanuary        { get; set; }
        public double TotalFebruary       { get; set; }
        public double TotalMarch          { get; set; }
        public double GrandTotalTime      { get; set; }
        public double GrandTotalCost      { get; set; }
        public double StandardHoursPerMonth { get; set; }
        public double TotalStandardHours { get; set; }
        public double GrandTotalPercentAllocated { get; set; }
        public double PercentAllocatedApril            { get; set; }
        public double PercentAllocatedMay              { get; set; }
        public double PercentAllocatedJune             { get; set; }
        public double PercentAllocatedJuly             { get; set; }
        public double PercentAllocatedAugust           { get; set; }
        public double PercentAllocatedSeptember        { get; set; }
        public double PercentAllocatedOctober          { get; set; }
        public double PercentAllocatedNovember         { get; set; }
        public double PercentAllocatedDecember         { get; set; }
        public double PercentAllocatedJanuary          { get; set; }
        public double PercentAllocatedFebruary         { get; set; }
        public double PercentAllocatedMarch            { get; set; }
    }

    /// <summary>Wrapper returned by the API containing rows and pre-computed footer summary.</summary>
    public class WorkGroupTimeByJobCodeRes
    {
        public IEnumerable<WorkGroupTimeByJobCodeRowRes> Rows    { get; set; } = [];
        public WorkGroupTimeByJobCodeSummaryRes          Summary { get; set; } = new();
        public Pagination                           Pagination { get; set; } = new();
        public double                               HrsPaid    { get; set; }
    }
}
