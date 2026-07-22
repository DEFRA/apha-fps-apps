using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the re-plan staff grid (frmRM_RePlan — Section 2).
    /// KeyProperty = "StaffRowKey" which is rendered as "{ParentProject}|{WgGrade}".
    /// </summary>
    public class ResourceMgmtReplanGridItem
    {
        /// <summary>Composite row key: "{ParentProject}|{WgGrade}".</summary>
        [GridColumn(IsVisible = false)]
        public string? StaffRowKey { get; set; }

        [GridColumn(IsVisible = false)]
        public string? WgGrade { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ParentProject { get; set; }

        [Display(Name = "Workgroup")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Grade")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Plan Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [Display(Name = "Programme")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }
    }
}
