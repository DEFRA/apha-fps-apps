namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ResourceStaffAllocationDto
    {
        public string? WorkGroupGrade { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public double? HrsAvail { get; set; }
        public double ZtHours { get; set; }
        public double AppPlannedHours { get; set; }
        public double PlannedHours { get; set; }
        public double ChargeHours { get; set; }
        public double AppChargeHours { get; set; }
    }
}
