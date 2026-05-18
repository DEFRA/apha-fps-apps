using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyOutputLogViewModel
    {
        public DataGridConfig<MonthlyOutputLogItem> LogGrid { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> TestCodeOptions { get; set; } = new();
        public List<SelectListItem> ProjectOptions { get; set; } = new();
        public List<SelectListItem> ActionOptions { get; set; } = new()
        {
            new SelectListItem("Inserted", "I"),
            new SelectListItem("Deleted", "D"),
            new SelectListItem("Updated", "U")
        };
    }
}
