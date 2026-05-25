using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffPlanViewItem
    {
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProgramNo { get; set; }

        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? StaffId { get; set; }

        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }

        [GridColumn(Order = 9, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? PayCost { get; set; }

        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [GridColumn(Order = 11, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Order = 12, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WgGrade { get; set; }

        [GridColumn(Order = 13, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? PcGrade { get; set; }

        [GridColumn(Order = 14, Width = 70, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }
    }
}
