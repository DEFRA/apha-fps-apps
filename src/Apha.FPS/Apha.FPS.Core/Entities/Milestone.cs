namespace Apha.FPS.Core.Entities
{
    // Maps fps.milestone — cross-year table, no FpsYear query filter applied
    public class Milestone
    {
        public string Project { get; set; } = null!;
        public string MilestoneRef { get; set; } = null!;
        public string ObjectiveRef { get; set; } = null!;
        public string? MilestoneTitle { get; set; }
        public DateOnly? PlanDate { get; set; }
        public DateOnly? ActualDate { get; set; }
        public string? Comment { get; set; }
        public double? MonthNoFin { get; set; }
        public string? Year { get; set; }
        public int? FpsYear { get; set; }
    }
}