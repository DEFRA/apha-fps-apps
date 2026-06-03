namespace Apha.FPS.Core.Entities
{
    public class TestSupplierView
    {
        public string TestCode { get; set; } = null!;
        public string JobCode { get; set; } = null!;
        public string? ProjectManager { get; set; }
        public double? NoTests { get; set; }
        public decimal? TestPrice { get; set; }
        public decimal TestCost { get; set; }
        public string? ProjectStatus { get; set; }
    }
}
