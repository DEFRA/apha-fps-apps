namespace Apha.Common.Contracts.PACT
{
    public class TestPriceCheckRes
    {
        public string TestCode { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public double? NoTests { get; set; }
        public decimal? TestPrice { get; set; }
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public string? Program { get; set; }
        public string? Manager { get; set; }
        public string? Owner { get; set; }
        public short IsDefraProject { get; set; }
        public decimal? NormalPrice { get; set; }
        public bool IsZeroPrice { get; set; }
        public bool IsNotStandard { get; set; }
    }
}
