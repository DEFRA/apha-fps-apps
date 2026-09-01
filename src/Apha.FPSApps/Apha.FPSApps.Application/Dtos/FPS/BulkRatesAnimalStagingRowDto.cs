namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>Mirrors <c>BulkRatesAnimalStagingRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesAnimalStagingRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string? Species { get; set; }
        public string? SecurityLevel { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? DailyRateNew { get; set; }
        public decimal? DefraDailyRate { get; set; }
        public decimal? DefraDailyRateNew { get; set; }
        public bool? PlanByWeek { get; set; }
    }
}
