namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffJobViewModel
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }

        public DateTime? SysTimestamp { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
