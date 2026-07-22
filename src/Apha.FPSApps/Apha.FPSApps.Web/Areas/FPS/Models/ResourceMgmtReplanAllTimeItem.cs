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
        public string? StaffId { get; set; }

        [Display(Name = "Staff")]
        public string? Name { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double PlannedHours { get; set; }
    }
}
