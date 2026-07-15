namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// API response contract for a staff allocation row in the Stage 2
    /// Check Resource Allocation grid (fsubResourceTotals2).
    /// </summary>
    public class ResourceStaffAllocationRes
    {
        public string? WorkGroupGrade { get; set; }
        public int? StaffId { get; set; }
        public string? Name { get; set; }
        public double? HoursAvailable { get; set; }
        public double? PlannedHours { get; set; }
        public double? AllocationPct { get; set; }
        public double? AssuredChargeHours { get; set; }
        public double? AssuredUtilisationPct { get; set; }
        public double? ChargeHours { get; set; }
        public double? UtilisationPct { get; set; }
    }
}
