namespace Apha.Common.Contracts.PIMS
{
    public class TestCostRes
    {
        // Plan (from my_tlkptestreqmt)
        public short Year { get; set; }
        public string? Buyer { get; set; }
        public string? TestCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? NoRequired { get; set; }
        public decimal? Cost { get; set; }

        // Actuals (from my_monthlyoutput joined to my_tlkptestreqmt)
        public double? Month { get; set; }
        public string? WorkGroup { get; set; }
        public double? Volume { get; set; }
        public decimal? Charge { get; set; }
    }
}
