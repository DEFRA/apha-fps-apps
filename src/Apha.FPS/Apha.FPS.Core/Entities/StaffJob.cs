namespace Apha.FPS.Core.Entities
{
    public partial class StaffJob
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }      

        public int? FpsYear { get; set; }
    }
}


