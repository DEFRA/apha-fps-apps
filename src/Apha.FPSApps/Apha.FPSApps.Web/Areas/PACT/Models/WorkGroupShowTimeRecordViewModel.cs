using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for the Work Group Show Time Records filter page.
    /// </summary>
    public class WorkGroupShowTimeRecordViewModel
    {
        /// <summary>Selected work group name.</summary>
        public string? SelectedWorkGroup { get; set; }

        /// <summary>Work group dropdown options.</summary>
        public List<WorkGroup> WorkGroupOptions { get; set; } = new List<WorkGroup>();

        /// <summary>Calendar month dropdown options.</summary>
        public List<CalenderMonth> CalenderMonthOptions { get; set; } = new List<CalenderMonth>();

        /// <summary>Data grid configuration for time records.</summary>
        public DataGridConfig<WorkGroupTimeCodeItem> TimeRecordsGrid { get; set; } = new DataGridConfig<WorkGroupTimeCodeItem>();
    }
}
