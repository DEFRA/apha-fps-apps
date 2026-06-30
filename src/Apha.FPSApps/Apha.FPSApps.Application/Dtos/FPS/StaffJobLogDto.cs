namespace Apha.FPSApps.Application.Dtos.FPS
{
    // Same shape as backend DTO — all 8 columns from fps.staffjob_log audit trail table
    public class StaffJobLogDto
    {
        public int SequenceNo { get; set; }
        public string StaffId { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public double PlannedHours { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
