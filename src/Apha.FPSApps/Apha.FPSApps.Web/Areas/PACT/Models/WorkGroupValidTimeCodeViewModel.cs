using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for the Work Group Valid Time Code filter page.
    /// </summary>
    public class WorkGroupValidTimeCodeViewModel
    {
        /// <summary>Selected work group name.</summary>
        public string SelectedWorkGroup { get; set; } = string.Empty;

        /// <summary>Work group dropdown options.</summary>
        public List<WorkGroup> WorkGroupOptions { get; set; } = new List<WorkGroup>();

        /// <summary>Data grid configuration for valid time codes.</summary>
        public DataGridConfig<WorkGroupValidTimeCodeItem> ValidTimeCodesGrid { get; set; } = new DataGridConfig<WorkGroupValidTimeCodeItem>();
    }
}