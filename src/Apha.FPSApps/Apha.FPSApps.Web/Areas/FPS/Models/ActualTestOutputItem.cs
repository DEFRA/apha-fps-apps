using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ActualTestOutputItem
    {
        [GridColumn(IsVisible = false)]
        public string? Buyer { get; set; }

        [Display(Name = "Test")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public double? Month { get; set; }

        [Display(Name = "Number")]
        [NonFinancialRange]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? Volume { get; set; }

        [Display(Name = "Rate")]
        [GridColumn(Width = 80, Type = GridColumnType.GbpValue)]
        public double? TestPrice { get; set; }

        [Display(Name = "Charge")]
        [GridColumn(Width = 80, Type = GridColumnType.GbpValue)]
        public double? Charge { get; set; }

        /// <summary>Composite key used by the grid delete action: TestCode|Buyer|Month|WorkGroup</summary>
        [GridColumn(IsVisible = false)]
        public string RowKey =>
            $"{TestCode ?? ""}|{Buyer ?? ""}|{Month?.ToString() ?? "0"}|{WorkGroup ?? ""}";
    }
}