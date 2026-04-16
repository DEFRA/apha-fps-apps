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
        public ProposedProjectDto? ProposedProjectDetails { get; set; }
        public ProjectDetailDto? ProjectDetails { get; set; }

        // Proposed Project Details (editable tab)
        public string? TransferTo { get; set; }
        public string? Projecttitle { get; set; }
        public string? Costbookno { get; set; }
        public string? Disease { get; set; }
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Manager { get; set; }

        // FPS Yearly Details (read-only tab)
        public List<ProjectsDto> YearlyDetails { get; set; } = new();

        // PIMS Details form (right panel)
        public string? Version { get; set; }
        public string? FileRef { get; set; }
        public string? CustomerRef { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CostbookNumber { get; set; }
        public int? Riskid { get; set; }
        public bool UseProjectYears { get; set; }
        public DateOnly? RevisedEndDate { get; set; }
        public DateOnly? ClosedDate { get; set; }

        // Comments grid
        public DataGridConfig<ProjectCommentItem> CommentsGrid { get; set; } = new();
        public int? SelectedCommentYear { get; set; }

        // Dropdown sources
        public List<SelectListItem> TransferToOptions { get; set; } = [];
        public List<SelectListItem> RiskRatingOptions { get; set; } = [];
        public List<SelectListItem> TopicOptions { get; set; } =
        [
            new SelectListItem("Select a topic", ""),
            new SelectListItem("A&F Report", "A&F Report"),
            new SelectListItem("Contracts", "Contracts"),
            new SelectListItem("General Comment", "General Comment"),
            new SelectListItem("Invoicing", "Invoicing"),
            new SelectListItem("Outturn Report", "Outturn Report"),
            new SelectListItem("P&C Monitoring Report", "P&C Monitoring Report")
        ];
        public List<SelectListItem> YearOptions { get; set; } = new();
    }
}
