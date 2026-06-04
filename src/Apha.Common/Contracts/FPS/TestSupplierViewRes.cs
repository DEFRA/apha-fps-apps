namespace Apha.Common.Contracts.FPS
{
    public class TestSupplierViewRes
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string? ProjectManager { get; set; }
        public int? NoRequired { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TestCost { get; set; }
        public string? ProjectStatus { get; set; }
    }
}
