namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class StaffJobDto
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }        

        public int? FpsCalYear { get; set; }
    }
}
