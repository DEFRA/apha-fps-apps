using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeLiveItem
    {
        [GridColumn(IsVisible = false)]
        public string CompositeKey { get; set; } = string.Empty;

        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text)]
        public string? WorkGroup { get; set; }

        [Display(Name = "PACT Staff Id")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text)]
        public string PactStaffId { get; set; } = string.Empty;

        [Display(Name = "Name")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Text)]
        public string? Name { get; set; }

        [Display(Name = "Time Code")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Text)]
        public string TimeCode { get; set; } = string.Empty;

        [Display(Name = "Parent Project")]
        [GridColumn(Order = 5, Width = 140, Type = GridColumnType.Text)]
        public string ParentProject { get; set; } = string.Empty;

        [Display(Name = "Period")]
        [GridColumn(Order = 6, Width = 80, Type = GridColumnType.Number)]
        public double Month { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.Number)]
        public double? Hours { get; set; }

        [GridColumn(IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
