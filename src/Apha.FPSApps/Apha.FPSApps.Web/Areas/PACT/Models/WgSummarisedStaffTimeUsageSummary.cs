namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Pre-computed footer totals for the Work Group Time By Job Code view.
    /// Populated from the service layer — no calculation in the view.
    /// </summary>
    public class WgSummarisedStaffTimeUsageSummary
    {
        // Row 1 — Total time per month
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

        // Row 2 — Standard hours per month: HrsPaid / 12
        public double StandardHoursPerMonth { get; set; }
        // Field66: sum of per-month standard hours (only months with data contribute)
        public double TotalStandardHours { get; set; }

        // Grand total % allocated: GrandTotalTime / (StandardHoursPerMonth * 12) * 100
        // mirrors Access Field80 = [field51] / [field66]
        public double GrandTotalPercentAllocated { get; set; }

        // Row 3 — % of standard hours allocated per month
        public double PercentAllocatedApril              { get; set; }
        public double PercentAllocatedMay                { get; set; }
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
}