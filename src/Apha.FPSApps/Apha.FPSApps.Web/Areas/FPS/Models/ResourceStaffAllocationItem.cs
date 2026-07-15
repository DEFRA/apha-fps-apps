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

        [GridColumn(IsVisible = false)]
        public int? StaffId { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 210, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Hrs Avail")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? HoursAvailable { get; set; }

        [Display(Name = "Plan Hr")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [Display(Name = "Aloc")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? AllocationPct { get; set; }

        [Display(Name = "App Chrg Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? AssuredChargeHours { get; set; }

        [Display(Name = "App Util")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? AssuredUtilisationPct { get; set; }

        [Display(Name = "Chrg Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? ChargeHours { get; set; }

        [Display(Name = "Util")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? UtilisationPct { get; set; }
    }
}
