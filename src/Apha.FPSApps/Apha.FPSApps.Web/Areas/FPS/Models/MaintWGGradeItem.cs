using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item model for WorkgroupGrade maintenance DataGrid.
    /// </summary>
    public class MaintWGGradeItem
    {
        /// <summary>WG Grade code — row key; shown in grid and used as KeyProperty.</summary>
        [Display(Name = "WGGrade")]
        [Required(ErrorMessage = "WGGrade is required")]
        [StringLength(50, ErrorMessage = "WGGrade cannot exceed 50 characters")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WgGrade { get; set; } = null!;

        /// <summary>Profit Centre Grade code.</summary>
        [Display(Name = "PCGrade")]
        [Required(ErrorMessage = "PCGrade is required")]
        [StringLength(20, ErrorMessage = "PCGrade cannot exceed 20 characters")]
        [GridColumn(Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentreGrade { get; set; } = null!;

        /// <summary>Grade code.</summary>
        [Display(Name = "Grade")]
        [Required(ErrorMessage = "Grade is required")]
        [StringLength(50, ErrorMessage = "Grade cannot exceed 50 characters")]
        [GridColumn(Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string GradeCode { get; set; } = null!;

        /// <summary>Workgroup name.</summary>
        [Display(Name = "WG")]
        [Required(ErrorMessage = "WG is required")]
        [StringLength(50, ErrorMessage = "WG cannot exceed 50 characters")]
        [GridColumn(Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string Workgroup { get; set; } = null!;
    }
}
