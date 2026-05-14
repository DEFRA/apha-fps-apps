namespace Apha.FPS.Core.Entities
{
    public class MonthlyTime
    {
        public string PactStaffId { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
        public double Month { get; set; }
        public string ParentProject { get; set; } = null!;
        public int FpsYear { get; set; }
        public double? Hours { get; set; }
        public string? WorkGroup { get; set; }
    }
}
