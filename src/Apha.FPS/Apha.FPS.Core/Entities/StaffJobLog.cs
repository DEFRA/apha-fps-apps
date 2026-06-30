namespace Apha.FPS.Core.Entities
{
    public partial class StaffJobLog
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
