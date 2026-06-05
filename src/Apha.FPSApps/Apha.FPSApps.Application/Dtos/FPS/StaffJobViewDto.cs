namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class StaffJobViewDto
    {
        public string? StaffID { get; set; }
        public string? JobCode { get; set; }
        public double PlannedHours { get; set; }
        public string? Name { get; set; }
        public string? WorkGroupGrade { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? StaffCost { get; set; }
        public string? GradeCode { get; set; }
        public string? WorkGroup { get; set; }
        public string? SectorName { get; set; }
        public double Days { get; set; }
        public string? ZtDescription { get; set; }
    }
}
