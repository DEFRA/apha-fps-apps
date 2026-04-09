using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupTestCapabilityViewModel
    {
        public string? SelectedWorkGroup { get; set; }
        public string? SelectedTestCode { get; set; }
        public int ViewBy { get; set; } = 1;

        public DataGridConfig<WorkGroupTestCapabilityItem> TestCapabilityGrid { get; set; } = new();
        public DataGridConfig<TestRequirementItem> TestReqmtGrid { get; set; } = new();

        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> TestorProductOptions { get; set; } = new();
    }
}
