using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeLogItem
    {
        [Display(Name = "ID")]
        [GridColumn(Order = 1, Width = 70, Type = GridColumnType.Number)]
        public int SequenceNo { get; set; }

        [Display(Name = "Time Code")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Project")]
        [GridColumn(Order = 3, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Month")]
        [GridColumn(Order = 4, Width = 70, Type = GridColumnType.Number)]
        public double Month { get; set; }

        [Display(Name = "Staff ID")]
        [GridColumn(Order = 5, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string PactStaffId { get; set; } = null!;

        [Display(Name = "WG")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Order = 7, Width = 70, Type = GridColumnType.Number)]
        public double? Hours { get; set; }

        [Display(Name = "Date Imported")]
        [GridColumn(Order = 8, Width = 150, Type = GridColumnType.DateTime)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "MAB User SP No.")]
        [GridColumn(Order = 9, Width = 150, Type = GridColumnType.Text)]
        public string? UserId { get; set; }

        [Display(Name = "Action")]
        [GridColumn(Order = 10, Width = 80, Type = GridColumnType.Text)]
        public string? InsertDelete { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
