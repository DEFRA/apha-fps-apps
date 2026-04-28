namespace Apha.Common.Contracts.PACT
{
    public class TestorProductReq
    {
        public string ItemCode { get; set; } = null!;
        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }
        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }
        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }
        public decimal DefraUnitPrice { get; set; }
        public int FpsYear { get; set; }
    }
}
