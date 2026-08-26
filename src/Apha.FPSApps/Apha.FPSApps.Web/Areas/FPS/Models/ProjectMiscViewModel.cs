using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Misc Project Data maintenance page.
    /// </summary>
    public class ProjectMiscViewModel
    {
        /// <summary>
        /// DataGrid configuration for the miscellaneous project data list.
        /// </summary>
        public DataGridConfig<ProjectMiscItem> ProjectMiscGrid { get; set; } = new DataGridConfig<ProjectMiscItem>();
    }

    /// <summary>
    /// Grid row and edit model for miscellaneous project data.
    /// </summary>
    public class ProjectMiscItem
    {
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "Project is required")]
        [MaxLength(20)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Program")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        [MaxLength(10)]
        public string? Program { get; set; }

        [Display(Name = "CostCentre")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.Number, IsFilterable = true)]
        public double? CostCentre { get; set; }

        [Display(Name = "OracleProjectCode")]
        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        [MaxLength(50)]
        public string? OracleProjectCode { get; set; }

        [Display(Name = "SubAccountCode")]
        [GridColumn(Order = 5, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        [MaxLength(50)]
        public string? SubAccountCode { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> SubAccountCodeList { get; set; } = new List<SelectListItem>();
    }
}
