using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PactProgramMaintenanceViewModel
    {
        public string SelectedProgramNo { get; set; } = string.Empty;
        public List<SelectListItem> ProgramList { get; set; } = [];
        public PactProgramViewModel Program { get; set; } = new();
        public DataGridConfig<PactProgramProjectItem> ProjectsGrid { get; set; } = new();
    }

    public class PactProgramViewModel
    {
        [Required(ErrorMessage = "Program number is required")]
        [StringLength(10, ErrorMessage = "Program number cannot exceed 10 characters")]
        [Display(Name = "Program No")]
        public string ProgramNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Program name is required")]
        [StringLength(80, ErrorMessage = "Program name cannot exceed 80 characters")]
        [Display(Name = "Program Name")]
        public string ProgramName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Sector name cannot exceed 100 characters")]
        [Display(Name = "Sector Name")]
        public string? SectorName { get; set; }

        [StringLength(100, ErrorMessage = "Customer cannot exceed 100 characters")]
        [Display(Name = "Customer")]
        public string? Customer { get; set; }

        [StringLength(50, ErrorMessage = "Leader cannot exceed 50 characters")]
        [Display(Name = "Leader")]
        public string? Manager { get; set; }

        [StringLength(20, ErrorMessage = "MINIM cannot exceed 20 characters")]
        [Display(Name = "MINIM")]
        public string? Minim { get; set; }

        [StringLength(50, ErrorMessage = "Directorate cannot exceed 50 characters")]
        [Display(Name = "Directorate")]
        public string? Directorate { get; set; }
    }

    public class PactProgramProjectItem
    {
        [Display(Name = "Code")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Title")]
        [GridColumn(Width = 300, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "BudgCVL")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "BudgExt")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "ProjectStatus")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly)]
        public string? ProjectStatus { get; set; }
    }
}
