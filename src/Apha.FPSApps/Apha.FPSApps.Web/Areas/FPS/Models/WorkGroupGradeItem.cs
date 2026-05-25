using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the WG Grades Available table (fsubWGGrade — delete only, no add/edit).
    /// </summary>
    public class WorkGroupGradeItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string ProfitCentreGrade { get; set; } = null!;

        [Display(Name = "WGGrade")]
        [GridColumn(Width = 250, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string WgGrade { get; set; } = null!;
    }
}
