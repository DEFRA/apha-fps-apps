namespace Apha.FPS.Core.Entities
{
    // Maps fps.projectmonth — cross-year table, no FpsYear query filter applied
    public class ProjectMonth
    {
        public string Project { get; set; } = null!;
        public int MonthNo { get; set; }
        public decimal? CostProfile { get; set; }
        public int? FpsYear { get; set; }
    }
}