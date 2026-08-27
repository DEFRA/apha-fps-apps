using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class CompareStaff2Item
    {
        [GridColumn(IsVisible = false)]
        public string? StaffId { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Project { get; set; }

        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Width = 70, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly)]
        public double? Month { get; set; }

        [NonFinancialRange]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? Time { get; set; }

        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }

        /// <summary>Composite key used by the grid delete action: workgroup|jobcode|project|month|staffid</summary>
        [GridColumn(IsVisible = false)]
        public string RowKey =>
            $"{WorkGroup ?? ""}|{JobCode ?? ""}|{Project ?? ""}|{Month?.ToString() ?? "0"}|{StaffId ?? ""}";
    }
}
