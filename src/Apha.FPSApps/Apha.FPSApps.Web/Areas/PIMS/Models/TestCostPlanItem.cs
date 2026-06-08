using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class TestCostPlanItem
    {
        [Display(Name = "Test Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly)]
        public string? TestCode { get; set; }

        [Display(Name = "Buyer")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Buyer { get; set; }

        [Display(Name = "Unit Price")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "No. Required")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly)]
        public double? NoRequired { get; set; }

        [Display(Name = "Cost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }
    }
}
