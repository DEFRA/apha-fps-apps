using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    // qryDeptIncomeTotals — PIVOT query, one row per project with area cost subtotals
    public class DepartmentIncomeTotalsItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        [Display(Name = "OracleProject")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OracleProjectCode { get; set; }

        // Maps to JS 'totalCost' column (width: 110)
        [Display(Name = "TotalCosts")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        // Maps to JS 'pay' column proxy (width: 100) for totals display
        [Display(Name = "TimeCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TimeCost { get; set; }

        // Maps to JS 'nonPay' column proxy (width: 100) for totals display
        [Display(Name = "TestsCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? TestsCost { get; set; }

        // Maps to JS 'overhead' column proxy (width: 100) for totals display
        [Display(Name = "AnimalsCost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AnimalsCost { get; set; }

        // Maps to JS 'chargeRate' column proxy (width: 110) for totals display
        [Display(Name = "ProjectSpecificsCost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ProjectSpecificsCost { get; set; }
    }
}
