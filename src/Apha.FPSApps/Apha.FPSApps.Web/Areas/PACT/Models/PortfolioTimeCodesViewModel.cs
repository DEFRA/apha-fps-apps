using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PortfolioTimeCodesViewModel
    {
        public string? SelectedPortfolio { get; set; }
        public List<SelectListItem> PortfolioOptions { get; set; } = [];
        public List<SelectListItem> WorkGroups { get; set; } = [];
        public DataGridConfig<JobCodeViewModel> JobCodeGrid { get; set; } = new DataGridConfig<JobCodeViewModel>();
        public DataGridConfig<TimeCodeValidityViewModel> TimeCodeGrid { get; set; } = new DataGridConfig<TimeCodeValidityViewModel>();
    }
}
