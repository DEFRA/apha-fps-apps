namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// DTO for Profit Centre with aggregated cost from TimeCostCalcs.
    /// </summary>
    public class ProfitCentreCostDto
    {
        public string ProfitCentre { get; set; } = null!;
        public decimal Cost { get; set; }
    }
}
