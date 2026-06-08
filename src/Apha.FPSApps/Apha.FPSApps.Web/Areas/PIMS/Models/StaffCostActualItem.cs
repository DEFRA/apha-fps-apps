using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class StaffCostActualItem
    {
        [Display(Name = "Job Code")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly)]
        public string? JobCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly)]
        public string? Name { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Grade")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public string? GradeCode { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [Display(Name = "Time")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public double? Time { get; set; }

        [Display(Name = "Rate")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [Display(Name = "Cost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? ActualCost { get; set; }
    }
}
