namespace Apha.FPS.Application.Dtos
{
    public class ProjectStaffPlanViewDto
    {
        public string ParentProject { get; set; } = null!;
        public string? ProgramNo { get; set; }
        public string? Contract { get; set; }
        public string? Name { get; set; }
        public string? StaffId { get; set; }
        public double? PlannedHours { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? Cost { get; set; }
        public decimal? PayCost { get; set; }
        public string? ProfitCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? WgGrade { get; set; }
        public string? PcGrade { get; set; }
        public string? GradeCode { get; set; }
    }
}
