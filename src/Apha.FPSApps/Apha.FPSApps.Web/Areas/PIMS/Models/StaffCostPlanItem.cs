using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class StaffCostPlanItem
    {
        [Display(Name = "WG Grade")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly)]
        public string? WgGrade { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly)]
        public string? Name { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [Display(Name = "Rate")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Rate { get; set; }

        [Display(Name = "Cost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }
    }
}
