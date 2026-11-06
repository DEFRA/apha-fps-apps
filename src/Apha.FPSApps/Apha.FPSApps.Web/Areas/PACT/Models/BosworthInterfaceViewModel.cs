using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class BosworthInterfaceViewModel
    {
        public List<SelectListItem> WorkGroupOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProfitCentreOptions { get; set; } = new List<SelectListItem>();
        public DataGridConfig<WgTestCapabilitiesWithDescriptionItem> CapabilitiesGrid { get; set; } = new DataGridConfig<WgTestCapabilitiesWithDescriptionItem>();
        public string? WorkGroup { get; set; }
    }
}