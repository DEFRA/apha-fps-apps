using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WorkGroupStaffMaintenanceViewModel
    {
        public DataGridConfig<WorkGroupEmployeeItem> WGStaffGrid { get; set; } = new DataGridConfig<WorkGroupEmployeeItem>();
    }
}
