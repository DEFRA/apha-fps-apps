namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response model for Profit Centre with aggregated cost.
    /// </summary>
    public class ProfitCentreCostRes
    {
        public string ProfitCentre { get; set; } = null!;
        public decimal Cost { get; set; }
    }
}
