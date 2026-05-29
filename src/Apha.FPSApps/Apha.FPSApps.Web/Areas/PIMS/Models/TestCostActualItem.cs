using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class TestCostActualItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly)]
        public string? TestCode { get; set; }

        [Display(Name = "Buyer")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsVisible =false)]
        public string? Buyer { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Volume")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? Volume { get; set; }

        [Display(Name = "Unit Price")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Charge")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Charge { get; set; }
    }
}
