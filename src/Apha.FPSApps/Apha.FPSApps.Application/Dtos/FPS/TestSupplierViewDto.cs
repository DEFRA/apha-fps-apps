namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class TestSupplierViewDto
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
