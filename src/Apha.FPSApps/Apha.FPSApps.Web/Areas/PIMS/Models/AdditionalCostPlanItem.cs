using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AdditionalCostPlanItem
    {
        [Display(Name = "Account")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly)]
        public string? Account { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly)]
        public string? Description { get; set; }

        [Display(Name = "Item Cost")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? ItemCost { get; set; }
    }
}
