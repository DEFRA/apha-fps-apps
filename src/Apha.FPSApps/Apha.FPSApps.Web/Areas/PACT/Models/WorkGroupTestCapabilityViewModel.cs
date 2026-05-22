using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for WorkGroup-focused Test Capability view.
    /// Based on the workgroup_show_valid_testoutput.html prototype.
    /// </summary>
    public class WorkGroupTestCapabilityViewModel
    {

        /// <summary>
        /// Grid configuration for WorkGroup Test Capabilities
        /// </summary>
        public DataGridConfig<WorkGroupTestCapabilityItem> TestCapabilityGrid { get; set; } = new();

        /// <summary>
        /// Available WorkGroup options for dropdown
        /// </summary>
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();

    }
}
