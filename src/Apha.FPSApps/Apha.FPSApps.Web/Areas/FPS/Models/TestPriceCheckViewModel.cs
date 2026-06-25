using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestPriceCheckViewModel
    {
        public DataGridConfig<TestPriceCheckItem> PriceCheckGrid { get; set; } = new();
        public List<string> Owners { get; set; } = new();
        public string SelectedPriceFilter { get; set; } = "all";
        public string? SelectedOwner { get; set; }
    }
}
