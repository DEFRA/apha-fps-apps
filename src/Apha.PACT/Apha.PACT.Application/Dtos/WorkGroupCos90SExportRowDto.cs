namespace Apha.PACT.Application.Dtos
{
    public class WorkGroupCos90SExportRowDto
    {
        public string WorkGroupName { get; set; } = string.Empty;
        public string ProfitCentre { get; set; } = string.Empty;
        public string PactId { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public string TimeCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ParentProject { get; set; } = string.Empty;
        public string? GradeCode { get; set; }
        public string? SpNumber { get; set; }
        public double? Hours { get; set; }
        public short Month { get; set; }
        public short Year { get; set; }
    }
}
