using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // JS getQueryColumns(): 18 columns, all read-only (no edit/delete/add buttons in modal grid)
    public class DepartmentIncomeTimeItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        // DTO field: OracleProjectCode (AP prefix + project code)
        [Display(Name = "Oracle Project Code")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        // DTO field: SubAccountCode
        [Display(Name = "Sub Account Code")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SubAccountCode { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        [Display(Name = "DefraProject")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? DefraProject { get; set; }

        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        [Display(Name = "SPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SPC { get; set; }

        [Display(Name = "SCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SCC { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "GradeCode")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "SPNumber")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SpNumber { get; set; }

        [Display(Name = "ChargeRate")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal ChargeRate { get; set; }

        [Display(Name = "Pay")]
        [GridColumn(Width = 100, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal Pay { get; set; }

        [Display(Name = "NonPay")]
        [GridColumn(Width = 100, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal NonPay { get; set; }

        [Display(Name = "Overhead")]
        [GridColumn(Width = 100, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal Overhead { get; set; }

        [Display(Name = "Time")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal Time { get; set; }

        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
