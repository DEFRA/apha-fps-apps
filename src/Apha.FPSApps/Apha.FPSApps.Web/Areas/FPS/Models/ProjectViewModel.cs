using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectViewModel
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Width = 250, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobDescription { get; set; }

        [Display(Name = "Programme")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string? Programme { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Budget_cvl { get; set; }

        [Display(Name = "Defra")]
        [GridColumn(Width = 55, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public short IsDefraProject { get; set; }
    }
}
