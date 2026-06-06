using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestCapabilityViewModel
    {
        public string? SelectedPortfolio { get; set; }
        public string? PortfolioDescription { get; set; }

        public List<SelectListItem> PortfolioOptions { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();

        public DataGridConfig<TestCapabilityItem> TestCapabilityGrid { get; set; } = new();
    }
}
