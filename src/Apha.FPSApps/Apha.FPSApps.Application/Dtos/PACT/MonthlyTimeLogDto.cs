namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyTimeLogDto
    {
        public int SequenceNo { get; set; }
        public string TimeCode { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public double Month { get; set; }
        public string PactStaffId { get; set; } = null!;
        public string? WorkGroup { get; set; }
        public double? Hours { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
