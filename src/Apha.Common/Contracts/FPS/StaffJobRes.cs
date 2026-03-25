namespace Apha.Common.Contracts.FPS
{
    public class StaffJobRes
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }

        public DateTime? SysTimestamp { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
