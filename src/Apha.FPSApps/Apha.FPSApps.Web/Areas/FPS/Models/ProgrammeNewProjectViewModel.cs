using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgrammeNewProjectViewModel
    {
        public string ParentProject { get; set; } = null!;
        public string ProjectTitle { get; set; } = null!;
        public string Program { get; set; } = null!;
        public string Customer { get; set; } = null!;
        public string? Manager { get; set; }
        public decimal TransferIncome { get; set; }
        public decimal CustIncome { get; set; }
        public string ProjectStatus { get; set; } = null!;
        public string? CostBookNo { get; set; }
        public decimal? Profit { get; set; }
        public decimal? BudgetCvl { get; set; }
        public string Disease { get; set; } = null!;
        public string Contract { get; set; } = null!;
        public string? ShortTitle { get; set; }
        public decimal? PvsIncome { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
        public string? Comments { get; set; }
        public decimal? CarryOver { get; set; }
        public decimal? CarryOverSeed { get; set; }
        public short IsDefraProject { get; set; }
        public double? CostCentre { get; set; }
        public string? OwningRc { get; set; }
        public string? ProjectGroup { get; set; }
        public string IncomeAccountCode { get; set; } = null!;
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
