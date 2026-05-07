using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectAnimalPlanActualViewModel
    {
        public string SelectedProjectCode { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public string Contract { get; set; } = string.Empty;

        public decimal TotalPlannedCost { get; set; }

        public decimal TotalActualCost { get; set; }

        public double PercentOfPlan { get; set; }

        public List<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();

        public DataGridConfig<AnimalPlanItem> AnimalPlanGrid { get; set; } = new DataGridConfig<AnimalPlanItem>();

        public DataGridConfig<ActualProjectCostItem> ActualAnimalCostGrid { get; set; } = new DataGridConfig<ActualProjectCostItem>();
    }
}
