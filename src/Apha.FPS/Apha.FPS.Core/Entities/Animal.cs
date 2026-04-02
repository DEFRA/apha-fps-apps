namespace Apha.FPS.Core.Entities
{
    public partial class Animal
    {
        public string AnimalType { get; set; } = null!;

        public string? Species { get; set; }

        public string? SecurityLevel { get; set; }

        public decimal? DailyRate { get; set; }

        public bool PlanByWeek { get; set; }

        public decimal? DefraDailyRate { get; set; }

        public int? FpsYear { get; set; }
    }
}