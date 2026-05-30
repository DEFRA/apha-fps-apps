using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectAdditionalPlanActualViewModel
    {
        public string SelectedProjectCode { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;

        public string Program { get; set; } = string.Empty;

        public string Contract { get; set; } = string.Empty;

        public decimal TotalPlannedCost { get; set; }

        public decimal TotalActualCost { get; set; }

        public double PercentOfPlan { get; set; }

        public List<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();

        public DataGridConfig<AdditionalCostItemViewModel> AdditionalCostPlanGrid { get; set; } = new DataGridConfig<AdditionalCostItemViewModel>();

        public DataGridConfig<ActualProjectCostItem> ActualAdditionalCostGrid { get; set; } = new DataGridConfig<ActualProjectCostItem>();
    }
}
