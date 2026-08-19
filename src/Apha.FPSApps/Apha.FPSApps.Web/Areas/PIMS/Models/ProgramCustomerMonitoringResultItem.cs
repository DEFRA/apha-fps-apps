using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProgramCustomerMonitoringResultItem
    {
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.ReadOnly)]
        public short? Year { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.ReadOnly)]
        public string? Project { get; set; }

        [Display(Name = "ParentProject")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.ReadOnly)]
        public string? ParentProject { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.ReadOnly)]
        public string? Program { get; set; }

        [Display(Name = "ProjectTitle")]
        [GridColumn(Order = 5, Width = 220, Type = GridColumnType.ReadOnly)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 6, Width = 180, Type = GridColumnType.ReadOnly)]
        public string? Manager { get; set; }

        [Display(Name = "ProjectStatus")]
        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.ReadOnly)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Order = 8, Width = 130, Type = GridColumnType.ReadOnly)]
        public string? Customer { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Order = 9, Width = 110, Type = GridColumnType.ReadOnly)]
        public string? Contract { get; set; }

        [Display(Name = "PlannedCosts")]
        [GridColumn(Order = 10, Width = 140, Type = GridColumnType.GbpValue)]
        public decimal? PlannedCosts { get; set; }

        [Display(Name = "Budget_CVL")]
        [GridColumn(Order = 11, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "CustIncome")]
        [GridColumn(Order = 12, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? CustIncome { get; set; }

        [Display(Name = "ActualCostsYT")]
        [GridColumn(Order = 13, Width = 140, Type = GridColumnType.GbpValue)]
        public decimal? ActualCostsYt { get; set; }

        [Display(Name = "PercentOfBudget")]
        [GridColumn(Order = 14, Width = 140, Type = GridColumnType.DecimalNumber)]
        public decimal? PercentOfBudget { get; set; }

        [Display(Name = "PCForecastSpend")]
        [GridColumn(Order = 15, Width = 140, Type = GridColumnType.DoubleNumber)]
        public double? PcForecastSpend { get; set; }

        [Display(Name = "EstimateSpend")]
        [GridColumn(Order = 16, Width = 130, Type = GridColumnType.DecimalNumber)]
        public decimal? EstimateSpend { get; set; }

        [Display(Name = "LinearSpend")]
        [GridColumn(Order = 17, Width = 130, Type = GridColumnType.DecimalNumber)]
        public decimal? LinearSpend { get; set; }

        [Display(Name = "BFBudget")]
        [GridColumn(Order = 18, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? BfBudget { get; set; }

        [Display(Name = "CumInvoice")]
        [GridColumn(Order = 19, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? CumInvoice { get; set; }

        [Display(Name = "StartDate")]
        [GridColumn(Order = 20, Width = 120, Type = GridColumnType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "EndDate")]
        [GridColumn(Order = 21, Width = 120, Type = GridColumnType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 22, Width = 220, Type = GridColumnType.ReadOnly)]
        public string? MonitoringComment { get; set; }
    }
}
