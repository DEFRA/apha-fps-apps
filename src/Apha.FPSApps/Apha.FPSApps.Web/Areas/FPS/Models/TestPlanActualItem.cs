using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestPlanActualItem
    {
        [Display(Name = "Test")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable = true, IsVisible = false)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Number")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.DecimalNumber)]
        public double? NoRequired { get; set; }

        [Display(Name = "Rate")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Charge")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = false)]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? TestCost => (UnitPrice ?? 0) * (decimal)(NoRequired ?? 0);

        [GridColumn(IsVisible = false)]
        public string Buyer { get; set; } = null!;
    }
}
