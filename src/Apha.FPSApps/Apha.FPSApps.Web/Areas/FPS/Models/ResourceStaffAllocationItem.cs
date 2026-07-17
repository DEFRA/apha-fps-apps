using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the Staff-of-Grade allocation grid (fsubResourceTotals2 — read-only).
    /// </summary>
    public class ResourceStaffAllocationItem
    {
        [GridColumn(IsVisible = false)]
        public string? WorkGroupGrade { get; set; }

        [GridColumn(IsVisible = true)]
        public string? StaffId { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 210, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Hrs Avail")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? HrsAvail { get; set; }

        [GridColumn(IsVisible = false)]
        public double ZtHours { get; set; }

        [Display(Name = "App Plan Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double AppPlannedHours { get; set; }

        [Display(Name = "Plan Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double PlannedHours { get; set; }

        [Display(Name = "Chrg Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double ChargeHours { get; set; }

        [Display(Name = "App Chrg Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double AppChargeHours { get; set; }
    }
}
