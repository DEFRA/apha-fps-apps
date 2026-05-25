using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for the Work Group Time By Job Code page (frmCluedo1 equivalent).
    /// </summary>
    public class WgSummarisedStaffTimeUsageViewModel
    {
        public string? SelectedWorkGroup { get; set; }

        /// <summary>Selected person name passed from the Work Group People page.</summary>
        public string? SelectedPersonName { get; set; }

        // Header fields (mirrors Access FormHeader: WorkGroup / Name / HrsPaid)

        /// <summary>Work group name displayed above the grid.</summary>
        public string? WorkGroupName { get; set; }

        /// <summary>Total HrsPaid for the work group.</summary>
        public double HrsPaid { get; set; }

        /// <summary>Grid of pivot rows driven by _DataGrid.</summary>
        public DataGridConfig<WgSummarisedStaffTimeUsageRow> Grid { get; set; } = new();

        /// <summary>Pre-computed footer totals (Total Time, Standard Hours, % Allocated).</summary>
        public WgSummarisedStaffTimeUsageSummary Summary { get; set; } = new();
    }
}
