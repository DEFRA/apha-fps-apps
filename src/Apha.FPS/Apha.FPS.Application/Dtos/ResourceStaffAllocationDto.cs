namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a single row in the Staff-of-Grade allocation grid
    /// (Access fsubResourceTotals2).
    /// </summary>
    public class ResourceStaffAllocationDto
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
