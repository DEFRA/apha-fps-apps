using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectProfitabilityViewModel
    {
        public string SelectedProgramNo { get; set; } = string.Empty;
        public List<SelectListItem> ProgrammeList { get; set; } = new();
        public string WorkTypeFilter { get; set; } = "all";
        public decimal? ProgrammeTarget { get; set; }
        public decimal ProgrammeSurplusShortfall { get; set; }
        public DataGridConfig<ProjectProfitabilityItem> ProfitabilityGrid { get; set; } = new();
    }
}
