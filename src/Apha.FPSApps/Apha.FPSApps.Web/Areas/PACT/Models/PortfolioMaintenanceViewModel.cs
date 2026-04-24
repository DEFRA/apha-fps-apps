using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PortfolioMaintenanceViewModel
    {
        public PortfolioDetailModel CurrentPortfolio { get; set; } = new();

        public List<SelectListItem> PortfolioOptions { get; set; } = [];
        public List<SelectListItem> Programs { get; set; } = [];
        public List<SelectListItem> Managers { get; set; } = [];
        public List<SelectListItem> WorkGroups { get; set; } = [];
        public List<SelectListItem> TestorProducts { get; set; } = [];

        public DataGridConfig<ConstituentTestItem> ConstituentTestGrid { get; set; } = new();
        public DataGridConfig<PortfolioTimeCodeViewModel> TimeCodeGrid { get; set; } = new();
    }
}
