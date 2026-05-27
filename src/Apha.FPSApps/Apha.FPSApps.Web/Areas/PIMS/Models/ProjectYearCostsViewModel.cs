using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectYearCostsViewModel
    {
        public string Parentproject { get; set; } = string.Empty;
        public short SelectedYear { get; set; }

        public List<SelectListItem> ProjectOptions { get; set; } = [];
        public List<SelectListItem> YearOptions { get; set; } = [];

        public DataGridConfig<AdditionalCostPlanItem> AdditionalPlansGrid { get; set; } = new();
        public DataGridConfig<AdditionalCostActualItem> AdditionalActualsGrid { get; set; } = new();
    }
}
