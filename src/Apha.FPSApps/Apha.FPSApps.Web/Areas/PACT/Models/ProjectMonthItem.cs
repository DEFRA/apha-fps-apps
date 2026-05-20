using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectMonthItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string Project { get; set; } = null!;

        [Display(Name = "Month No")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Number, IsFilterable = false)]
        public int MonthNo { get; set; }

        [Display(Name = "Cost Profile")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.GbpValue)]
        public decimal? CostProfile { get; set; }
    }
}