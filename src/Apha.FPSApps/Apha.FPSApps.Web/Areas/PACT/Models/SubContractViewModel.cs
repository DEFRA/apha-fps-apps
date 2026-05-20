using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractViewModel
    {
        public string ParentProject { get; set; } = string.Empty;
        public int? Month { get; set; }
        public DataGridConfig<SubContractItem> SubContractsGrid { get; set; } = new DataGridConfig<SubContractItem>();
        public List<SelectListItem> FilterProjects { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> FilterMonths { get; set; } = new List<SelectListItem>();
    }
}
