using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgrammeNewProjectViewModel
    {
        [Display(Name = "Parent Project")]
        public string ParentProject { get; set; } = null!;
        [Display(Name = "Project Title")]
        public string ProjectTitle { get; set; } = null!;
        public string Program { get; set; } = null!;
        public string Customer { get; set; } = null!;
        [Required]
        [Display(Name = "Manager")]
        public string? Manager { get; set; }
        [Display(Name = "Transfer Income")]
        public decimal TransferIncome { get; set; }
        [Display(Name = "Cust Income")]
        public decimal CustIncome { get; set; }
        [Display(Name = "Project Status")]
        public string ProjectStatus { get; set; } = null!;
        public string? CostBookNo { get; set; }
        public decimal? Profit { get; set; }
        [Required]
        [Display(Name = "Budget")]
        public decimal? BudgetCvl { get; set; }
        public string Disease { get; set; } = null!;
        public string Contract { get; set; } = null!;
        [Display(Name = "Short Title")]
        public string ShortTitle { get; set; } = null!;
        public decimal? PvsIncome { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
        public string? Comments { get; set; }
        public decimal? CarryOver { get; set; }
        public decimal? CarryOverSeed { get; set; }
        public short IsDefraProject { get; set; }
        [Required]
        [Display(Name = "Cost Centre")]
        public double? CostCentre { get; set; }
        public string? OwningRc { get; set; }
        public string? ProjectGroup { get; set; }
        [Display(Name = "Income Account Code")]
        public string IncomeAccountCode { get; set; } = null!;
        [Required]
        [Display(Name = "Objective Code")]
        public string? SubAccountCode { get; set; }
        public string? OracleProjectCode { get; set; }

        // Dropdown lists
        public string SelectedProgramNo { get; set; } = string.Empty;
        public List<SelectListItem> ProgrammeList { get; set; } = new();
        public List<SelectListItem> CustomerList { get; set; } = new();
        public List<SelectListItem> ProgramList { get; set; } = new();
        public List<SelectListItem> ManagerList { get; set; } = new();
        public List<SelectListItem> DiseaseList { get; set; } = new();
        public List<SelectListItem> ProjectStatusList { get; set; } = new();
        public List<SelectListItem> ContractList { get; set; } = new();
        public List<SelectListItem> CostCentreList { get; set; } = new();
        public List<SelectListItem> ProjectGroupList { get; set; } = new();
        public List<SelectListItem> IncomeAccountCodeList { get; set; } = new();
        public List<SelectListItem> SubAccountCodeList { get; set; } = new();
        public List<SelectListItem> IsDefraProjectList { get; set; } = new();
    }
}
