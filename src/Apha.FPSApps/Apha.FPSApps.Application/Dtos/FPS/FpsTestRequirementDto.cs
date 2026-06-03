namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class FpsTestRequirementDto
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public short? Active { get; set; }
    }
}
