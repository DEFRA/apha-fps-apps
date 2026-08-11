using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // qryDeptIncomeTotals — PIVOT query, one row per project with area cost subtotals.
    // Snapshot tab: 3 columns (Project, Oracle Project Code, TotalCosts)— subtotals hidden.
    // Current tab:  7 columns — subtotals made visible by the controller for that source.
    public class DepartmentIncomeTotalsItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        [Display(Name = "Oracle Project Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        [Display(Name = "TotalCosts")]
        [GridColumn(Width = 120, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Time")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false, IsVisible = false)]
        public decimal? TimeCost { get; set; }

        [Display(Name = "Tests")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false, IsVisible = false)]
        public decimal? TestsCost { get; set; }

        [Display(Name = "Animals")]
        [GridColumn(Width = 110, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false, IsVisible = false)]
        public decimal? AnimalsCost { get; set; }

        [Display(Name = "Project-specifics")]
        [GridColumn(Width = 130, Type = GridColumnType.RoundTwoDecimal, IsFilterable = false, IsVisible = false)]
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
