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

        [Display(Name = "Staff ID")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? StaffId { get; set; }

        [Display(Name = "Job Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Grade")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public string? GradeCode { get; set; }

        [Display(Name = "Plan Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double PlannedHours { get; set; }

        [Display(Name = "Days")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public double Days { get; set; }

        [Display(Name = "Charge Rate")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [Display(Name = "Staff Cost")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? StaffCost { get; set; }
    }
}
