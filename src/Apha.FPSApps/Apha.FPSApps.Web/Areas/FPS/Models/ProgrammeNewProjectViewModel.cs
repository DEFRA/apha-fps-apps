using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgrammeNewProjectViewModel
    {
        [Display(Name = "Parent Project")]
        [StringLength(20, ErrorMessage = "Project Code cannot exceed 20 characters.")]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Project Code must contain only letters (A-Z, a-z) and numbers (0-9).")]
        public string ParentProject { get; set; } = null!;
        [Display(Name = "Project Title")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string ProjectTitle { get; set; } = null!;
        public string Program { get; set; } = null!;
        public string Customer { get; set; } = null!;

        [Required]
        [Display(Name = "Manager")]
        public string? Manager { get; set; }

        [Display(Name = "Transfer Income")]
        [CurrencyRange]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Cust Income")]
        [CurrencyRange]
        public decimal CustIncome { get; set; }
        [Display(Name = "Project Status")]
        public string ProjectStatus { get; set; } = null!;

        [Display(Name = "CostBookNo")]
        [StringLength(50, ErrorMessage = "CostBookNo cannot exceed 50 characters.")]
        public string? CostBookNo { get; set; }

        [CurrencyRange]
        public decimal? Profit { get; set; }

        [Required]
        [Display(Name = "Budget")]
        [CurrencyRange]
        public decimal? BudgetCvl { get; set; }

        public string Disease { get; set; } = null!;
        public string Contract { get; set; } = null!;
        [Display(Name = "Short Title")]
        [StringLength(30, ErrorMessage = "Short Title cannot exceed 30 characters.")]
        public string ShortTitle { get; set; } = null!;
        [Display(Name = "PVS Income")]
        [CurrencyRange]
        public decimal? PvsIncome { get; set; }
        [Display(Name = "Plan Case Work Debit")]
        [CurrencyRange]
        public decimal? PlanCaseWorkDebit { get; set; }
        public string? Comments { get; set; }
        [Display(Name = "Carry Over")]
        [CurrencyRange]
        public decimal? CarryOver { get; set; }
        [Display(Name = "Carry Over Seed")]
        [CurrencyRange]
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
