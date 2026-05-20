using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ActualProjectCostItem
    {
        [GridColumn(IsVisible = false)]
        public int SubContCounter { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Project { get; set; }

        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Description { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AcctCode { get; set; }

        [Display(Name = "F Month")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
    }
}
