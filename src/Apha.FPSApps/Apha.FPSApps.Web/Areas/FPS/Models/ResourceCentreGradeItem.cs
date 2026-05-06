using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the RC Grades Available table (fsubpCGrade — read-only, no add/edit/delete).
    /// </summary>
    public class ResourceCentreGradeItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string PcGrade { get; set; } = null!;

        [Display(Name = "RCGrade")]
        [GridColumn(Width = 250, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string RcGradeDisplay { get; set; } = null!;

        [Display(Name = "ChargeRate")]
        [GridColumn(Width = 150, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ChargeRate { get; set; }
    }
}
