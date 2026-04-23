using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PactProgramMaintenanceViewModel
    {
        public string SelectedProgramNo { get; set; } = string.Empty;
        public List<SelectListItem> ProgramList { get; set; } = [];
        public ProgramViewModel Program { get; set; } = new();
        public DataGridConfig<ProgramProjectItem> ProjectsGrid { get; set; } = new();
    }
}
