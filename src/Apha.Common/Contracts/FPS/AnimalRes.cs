namespace Apha.Common.Contracts.FPS
{
    public class AnimalRes
    {
        public string AnimalType { get; set; } = null!;

        public string? Species { get; set; }

        public string? SecurityLevel { get; set; }

        public decimal? DailyRate { get; set; }

        public bool PlanByWeek { get; set; }

        public decimal? DefraDailyRate { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
