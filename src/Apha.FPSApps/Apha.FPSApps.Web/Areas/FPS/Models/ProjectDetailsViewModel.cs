using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectDetailsViewModel
    {
        [Display(Name = "Project Code")]
        public string ProjectCode { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Short Title")]
        public string? ShortTitle { get; set; }

        [Display(Name = "Customer")]
        public string? Customer { get; set; }

        [Display(Name = "Manager")]
        public string? Manager { get; set; }

        [Display(Name = "Disease")]
        public string? Disease { get; set; }

        [Display(Name = "Customer Income")]
        [DataType(DataType.Currency)]
        public decimal CustIncome { get; set; }

        [Display(Name = "Transfer Income")]
        [DataType(DataType.Currency)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Target Profit")]
        [DataType(DataType.Currency)]
        public decimal? TargetProfit { get; set; }

        [Display(Name = "Status")]
        public string? ProjectStatus { get; set; }

        [Display(Name = "CostBook No")]
        public string? CostBookNo { get; set; }

        [Display(Name = "Contract No")]
        public string? Contract { get; set; }

        [Display(Name = "Is Defra Project?")]
        public short IsDefraProject { get; set; }

        [Display(Name = "Cost Centre")]
        public double? CostCentre { get; set; }

        [Display(Name = "Resource Centre")]
        public string? OwningRc { get; set; }

        [Display(Name = "Project Group")]
        public string? ProjectGroup { get; set; }

        [Display(Name = "Income Account Code")]
        public string? IncomeAccountCode { get; set; }

        [Display(Name = "Objective Code")]
        public string? SubAccountCode { get; set; }

        [Display(Name = "Budget")]
        [DataType(DataType.Currency)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "PVS Income")]
        [DataType(DataType.Currency)]
        public decimal? PvsIncome { get; set; }

        [Display(Name = "Plan CW Debit")]
        [DataType(DataType.Currency)]
        public decimal? PlanCaseWorkDebit { get; set; }

        [Display(Name = "Carry Over")]
        [DataType(DataType.Currency)]
        public decimal? CarryOver { get; set; }

        [Display(Name = "Carry Over Seed")]
        [DataType(DataType.Currency)]
        public decimal? CarryOverSeed { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }
    }
}
