using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffPlanViewModel
    {
        public DataGridConfig<StaffPlanViewItem> Grid { get; set; } = new DataGridConfig<StaffPlanViewItem>();
    }
}
