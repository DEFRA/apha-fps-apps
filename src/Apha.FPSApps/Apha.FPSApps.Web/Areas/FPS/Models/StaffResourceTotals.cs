namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Aggregated "Staff Totals" for the Staff Resource Utilisation grid.
    /// These totals are always calculated from the complete workgroup dataset,
    /// independent of pagination, filtering, sorting, or the number of rows displayed.
    /// </summary>
    public class StaffResourceTotals
    {
        public double TotalH { get; set; }
        public double Ztw { get; set; }
        public double Avail { get; set; }
        public double Left { get; set; }
        public double ApprovedPlan { get; set; }
        public double NotApprovedPlan { get; set; }
        public double TotalPlan { get; set; }

        // Percentage columns are averaged across the rows that have a value,
        // mirroring the previous client-side behaviour.
        public double? ApprovedUtil { get; set; }
        public double? NotApprovedUtil { get; set; }
        public double? TotalUtil { get; set; }
    }
}
