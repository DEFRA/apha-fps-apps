using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectStaffPlanActualViewModel
    {
        public string SelectedProjectCode { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public string Contract { get; set; } = string.Empty;

        public decimal TotalPlannedCost { get; set; }

        public double TotalActualHrs { get; set; }

        public double TotalActualCost { get; set; }

        public double PercentOfPlan { get; set; }

        public List<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();

        public DataGridConfig<StaffJobItemViewModel> StaffPlanGrid { get; set; } = new DataGridConfig<StaffJobItemViewModel>();

        public DataGridConfig<CompareStaff2Item> CompareStaff2Grid { get; set; } = new DataGridConfig<CompareStaff2Item>();
    }
}
