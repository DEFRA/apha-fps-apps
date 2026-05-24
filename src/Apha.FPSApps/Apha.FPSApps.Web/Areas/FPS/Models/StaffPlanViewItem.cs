using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffPlanViewItem
    {
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProgramNo { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? StaffId { get; set; }

        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }

        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? PayCost { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ProfitCentre { get; set; }

        [GridColumn(IsVisible = false)]
        public string? WgGrade { get; set; }

        [GridColumn(IsVisible = false)]
        public string? PcGrade { get; set; }
    }
}
