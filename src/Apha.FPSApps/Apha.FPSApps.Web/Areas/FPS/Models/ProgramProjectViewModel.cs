using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramProjectViewModel
    {
        public string SelectedProgramNo { get; set; } = string.Empty;

        public string SelectedProgramme { get; set; } = string.Empty;

        public string SelectedProjectCode { get; set; } = string.Empty;

        public List<SelectListItem> ProgrammeList { get; set; } = new List<SelectListItem>();

        public DataGridConfig<ProgramProjectItem> ProjectsGrid { get; set; } = new DataGridConfig<ProgramProjectItem>();
    }
}
