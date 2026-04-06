using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramAnimalPlanViewModel
    {
        public string SelectedProgramNo { get; set; } = string.Empty;

        public string SelectedProgramme { get; set; } = string.Empty;

        public string Manager { get; set; } = string.Empty;

        public decimal Target { get; set; }

        public decimal AnimalCost { get; set; }

        public List<SelectListItem> ProgrammeList { get; set; } = new List<SelectListItem>();

        public DataGridConfig<ProjectViewModel> ProjectsGrid { get; set; } = new DataGridConfig<ProjectViewModel>();

        public DataGridConfig<AnimalPlanItem> AnimalCostGrid { get; set; } = new DataGridConfig<AnimalPlanItem>();
    }
}
