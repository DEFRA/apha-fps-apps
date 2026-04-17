using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectDetailsViewModel
    {
        public string Parentproject { get; set; } = string.Empty;

        // FPS Project Details (read-only tab)
        public ProjectDto? FpsProjectDetails { get; set; }

        public bool IsFPS { get; set; } = false;
        public bool IsProposedProjectUpdate { get; set; } = false;
        public ProposedProjectDto? ProposedProjectDetails { get; set; }
        public ProjectDetailDto? ProjectDetails { get; set; }

        // FPS Yearly Details (read-only tab)
        public List<ProjectsDto> YearlyDetails { get; set; } = new();

        // PIMS Details form (right panel)
        public bool UseProjectYears { get; set; }

        // Comments grid
        public DataGridConfig<ProjectCommentItem> CommentsGrid { get; set; } = new();
        public int? SelectedCommentYear { get; set; }

        // Dropdown sources
        public List<SelectListItem> TransferToOptions { get; set; } = [];
        public List<SelectListItem> RiskRatingOptions { get; set; } = [];

        public List<SelectListItem> YearOptions { get; set; } = new();
    }
}