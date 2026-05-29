namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for Profit Centre (Resource Centre) maintenance operations.
    /// </summary>
    public class ProfitCentreReq
    {
       
        public string ProfitCentreId { get; set; } = null!;
        public string ProfitCentreName { get; set; } = null!;
        public string Division { get; set; } = null!;
        public decimal? ContTarget { get; set; }
        public string? ProfitCentreHead { get; set; }
        public int? DivisionId { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
