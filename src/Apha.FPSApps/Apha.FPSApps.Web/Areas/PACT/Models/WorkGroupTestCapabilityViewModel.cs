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
        /// Currently selected WorkGroup name
        /// </summary>
        public string? SelectedWorkGroup { get; set; }

        /// <summary>
        /// Currently selected Portfolio (for future enhancement)
        /// </summary>
        public string? SelectedPortfolio { get; set; }

        /// <summary>
        /// Grid configuration for WorkGroup Test Capabilities
        /// </summary>
        public DataGridConfig<WorkGroupTestCapabilityItem> TestCapabilityGrid { get; set; } = new();

        /// <summary>
        /// Available WorkGroup options for dropdown
        /// </summary>
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();

        /// <summary>
        /// Available Portfolio options for dropdown (future enhancement)
        /// </summary>
        public List<SelectListItem> PortfolioOptions { get; set; } = new();

        /// <summary>
        /// Flag to show/hide Project Administration button
        /// </summary>
        public bool ShowProjectAdministration { get; set; } = false;
    }
}
