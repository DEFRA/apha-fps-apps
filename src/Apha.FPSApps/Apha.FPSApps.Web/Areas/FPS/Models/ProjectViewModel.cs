using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectViewModel
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Width = 250, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Programme")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string? Program { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Defra")]
        [GridColumn(Width = 55, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public short IsDefraProject { get; set; }

        // Edit form fields — not surfaced as grid columns
        public string? Manager { get; set; }
        public string? Customer { get; set; }
        public string? ProjectGroup { get; set; }
        public string? Contract { get; set; }
        public string? Disease { get; set; }
        public string? ProjectStatus { get; set; }
        public decimal? BudgetExt { get; set; }
        public decimal? TransferIncome { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }

        // Dropdown lists — populated in PopulateDropdownsAsync
        public List<SelectListItem> ManagerList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProgramList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectGroupList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ContractList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DiseaseList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();
    }
}
