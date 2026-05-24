using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeLogViewModel
    {
        public DataGridConfig<MonthlyTimeLogItem> LogGrid { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> TestCodeOptions { get; set; } = new();
        public List<SelectListItem> ProjectOptions { get; set; } = new();
        public List<SelectListItem> JobCodeOptions { get; set; } = new();
        public List<SelectListItem> ActionOptions { get; set; } = new()
        {
            new SelectListItem("Inserted", "I"),
            new SelectListItem("Deleted", "D"),
            new SelectListItem("Updated", "U")
        };
    }
}
