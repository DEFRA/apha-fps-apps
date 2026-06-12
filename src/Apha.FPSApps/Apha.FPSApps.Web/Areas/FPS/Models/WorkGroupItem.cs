using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WorkGroupItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string WorkGroupName { get; set; } = string.Empty;

        [Display(Name = "Work Group")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string WorkGroup { get; set; } = string.Empty;

        [Display(Name = "Profit Centre")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = true)]
        public string? Description { get; set; }

        [Display(Name = "FPS Year")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? FpsYear { get; set; }

        [Display(Name = "User")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = true)]
        public string? Dt2Username { get; set; }

        [Display(Name = "User Email")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsVisible = false, IsFilterable = true)]
        public string? UserEmail { get; set; }
    }
}
