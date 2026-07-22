using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the all-time staff jobs grid (frmRM_RePlan — Section 3).
    /// </summary>
    public class ResourceMgmtReplanAllTimeItem
    {
        [GridColumn(IsVisible = false)]
        public string? WgGrade { get; set; }

        [GridColumn(IsVisible = false)]
        public string? WorkGroupGrade { get; set; }

        [Display(Name = "Staff ID")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? StaffId { get; set; }

        [Display(Name = "Staff")]
        [GridColumn(IsVisible = true)]
        public string? Name { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double PlannedHours { get; set; }

        [Display(Name = "Days")]
        [GridColumn(IsVisible = false)]
        public double Days { get; set; }

        [Display(Name = "Charge Rate")]
        [GridColumn(IsVisible = false)]
        public decimal? ChargeRate { get; set; }

        [Display(Name = "Staff Cost")]
        [GridColumn(IsVisible = false)]
        public decimal? StaffCost { get; set; }
    }
}
