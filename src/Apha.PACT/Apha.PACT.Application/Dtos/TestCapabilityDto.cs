namespace Apha.PACT.Application.Dtos
{
    public class TestCapabilityDto
    {
        public string TestCode { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string PlanPortfolio { get; set; } = null!;
        public decimal? UnitCost { get; set; }
        public double? PredOutturn { get; set; }
        public string? Sop { get; set; }
        public string? SmsCode { get; set; }
        public int FpsYear { get; set; }
    }
}
