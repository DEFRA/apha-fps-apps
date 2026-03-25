using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffMaintenanceViewModel
    {
        public DataGridConfig<EmployeeViewModel> StaffGrid { get; set; } = new DataGridConfig<EmployeeViewModel>();
        public int? FilterOption { get; set; } = 1;
    }
}
