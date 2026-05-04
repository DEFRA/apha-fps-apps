using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectListItem
    {
        //[GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        //public string Parentproject { get; set; } = null!;

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string Parentproject { get; set; } = string.Empty;

        [Display(Name = "Program")]
        [GridColumn(Width = 120, Type = GridColumnType.Text)]
        public string? Program { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 200, Type = GridColumnType.Text)]
        public string? Customer { get; set; }

        [Display(Name = "On FPS")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public string? OnFps { get; set; }
    }
}
