using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class LogMilestoneViewModel
    {
        public string Parentproject { get; set; } = string.Empty;
        public List<SelectListItem> ProjectOptions { get; set; } = [];
        public DataGridConfig<LogMilestoneItem> LogMilestonesGrid { get; set; } = new();
    }
}
