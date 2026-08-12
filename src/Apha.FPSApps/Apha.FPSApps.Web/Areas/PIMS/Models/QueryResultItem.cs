using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class QueryResultItem
    {
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.ReadOnly)]
        public string Project { get; set; } = string.Empty;

        [Display(Name = "Contract")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.ReadOnly)]
        public string Contract { get; set; } = string.Empty;

        [Display(Name = "Manager")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.ReadOnly)]
        public string Manager { get; set; } = string.Empty;

        [Display(Name = "Status")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.ReadOnly)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Plan Costs")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal PlanCosts { get; set; }

        [Display(Name = "YTD Costs")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal YtdCosts { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 7, Width = 150, Type = GridColumnType.ReadOnly)]
        public string Comments { get; set; } = string.Empty;
    }
}
