using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectPlanViewerViewModel
    {
        // Selection filters
        public List<SelectListItem> ProgramList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectGroupList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectList { get; set; } = new List<SelectListItem>();

        public string SelectedProgram { get; set; } = string.Empty;
        public string SelectedProjectGroup { get; set; } = string.Empty;
        public string SelectedProjectCode { get; set; } = string.Empty;

        // Project Details grid (master grid showing filtered projects)
        public DataGridConfig<ProjectDetailsGridItem> ProjectDetailsGrid { get; set; } = new DataGridConfig<ProjectDetailsGridItem>();

        // Project-code-dependent details (partial view model)
        public ProjectDetailsPartialViewModel ProjectDetails { get; set; } = new ProjectDetailsPartialViewModel();
    }
}
