using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectTestPlanActualViewModel
    {
        public string SelectedProjectCode { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string Program { get; set; } = string.Empty;
        public string Contract { get; set; } = string.Empty;
        public decimal TotalPlannedCost { get; set; }
        public double TotalActualVolume { get; set; }
        public double TotalActualCost { get; set; }
        public double PercentOfPlan { get; set; }
        public List<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();
        public DataGridConfig<TestPlanActualItem> TestPlanGrid { get; set; } = new DataGridConfig<TestPlanActualItem>();
        public DataGridConfig<ActualTestOutputItem> CompareTests2Grid { get; set; } = new DataGridConfig<ActualTestOutputItem>();
    }
}