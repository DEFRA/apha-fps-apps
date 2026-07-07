using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WorkGroupStaffMaintenanceViewModel
    {
        public DataGridConfig<WorkGroupEmployeeStaffItem> WGStaffGrid { get; set; } = new DataGridConfig<WorkGroupEmployeeStaffItem>();
    }
}
