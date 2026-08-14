namespace Apha.PACT.Core.Entities
{
    public class TestRequirementDetail
    {
        public string TestCode { get; set; } = null!;
        public string? ItemDescription { get; set; }
        public string Buyer { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public short? Active { get; set; }
        public int FpsYear { get; set; }
        public short IsDefraProject { get; set; }
        public decimal? RecUnitPrice { get; set; }
        public decimal? TestCost { get; set; }
    }
}
