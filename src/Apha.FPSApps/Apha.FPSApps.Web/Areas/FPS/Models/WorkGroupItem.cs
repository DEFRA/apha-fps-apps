using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WorkGroupItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string WorkgroupName { get; set; } = string.Empty;

        [Display(Name = "Work Group")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string WorkGroup { get; set; } = string.Empty;
    }
}
