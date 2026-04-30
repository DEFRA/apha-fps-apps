using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class CompareTests2Item
    {
        [GridColumn(IsVisible = false)]
        public string? Buyer { get; set; }

        [Display(Name = "Test")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly)]
        public string? WorkGroup { get; set; }

        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [Display(Name = "Number")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
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