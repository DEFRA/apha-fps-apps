using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MilestoneViewModel
    {
        public string Parentproject { get; set; } = string.Empty;
        public List<SelectListItem> ProjectOptions { get; set; } = [];
        public List<SelectListItem> MilestoneTypeOptions { get; set; } = [];
        public DataGridConfig<MilestoneItem> MilestonesGrid { get; set; } = new();
        public DataGridConfig<MilestoneFormDatesItem> MilestoneFormDatesGrid { get; set; } = new();
        public bool FormRequired { get; set; }
    }
}
