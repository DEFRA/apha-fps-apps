using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectGroupStaffPlanViewItem
    {
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectGroup { get; set; }

        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ResourceCentre { get; set; }

        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [GridColumn(Order = 5, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [GridColumn(Order = 6, Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [GridColumn(Order = 8, Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [GridColumn(Order = 9, Width = 70, Type = GridColumnType.ReadOnly)]
        public double? Hrs { get; set; }

        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [GridColumn(Order = 11, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? Fee { get; set; }
    }
}
