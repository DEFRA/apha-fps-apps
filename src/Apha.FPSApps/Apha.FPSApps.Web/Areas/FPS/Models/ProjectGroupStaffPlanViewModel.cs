using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectGroupStaffPlanViewModel
    {
        public DataGridConfig<ProjectGroupStaffPlanViewItem> Grid { get; set; } = new DataGridConfig<ProjectGroupStaffPlanViewItem>();
    }
}
