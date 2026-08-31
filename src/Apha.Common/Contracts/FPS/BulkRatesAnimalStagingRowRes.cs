namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single Animal staging row on the "Animal Data (Staging)" grid.
    /// </summary>
    public class BulkRatesAnimalStagingRowRes
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
