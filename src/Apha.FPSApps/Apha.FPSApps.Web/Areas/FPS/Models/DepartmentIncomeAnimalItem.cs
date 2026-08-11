using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // JS getQueryColumns() modal shared grid — 13 of 18 columns present for this query type
    public class DepartmentIncomeAnimalItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        [Display(Name = "Oracle Project Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        [Display(Name = "Sub Account Code")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SubAccountCode { get; set; }

        [Display(Name = "DefraProject")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? DefraProject { get; set; }

        [Display(Name = "OPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OPC { get; set; }

        [Display(Name = "OCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OCC { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Month { get; set; }

        [Display(Name = "SPC")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SPC { get; set; }

        [Display(Name = "SCC")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SCC { get; set; }

        [Display(Name = "AnimalType")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AnimalType { get; set; }

        [Display(Name = "AnimalDays")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal AnimalDays { get; set; }

        [Display(Name = "Rate")]
        [GridColumn(Width = 100, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal Rate { get; set; }

        [Display(Name = "TotalCost")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal TotalCost { get; set; }
    }
}
