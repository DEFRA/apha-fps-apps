using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class YearEndInitiationViewModel
    {
        public int PlannedYear { get; set; }
        public bool CanRunJob { get; set; }
        public List<YearEndConfigValueItem> ConfigValues { get; set; } = [];
        public List<YearEndMonthWorkingItem> MonthWorkingHours { get; set; } = [];
        public required DataGridConfig<YearEndHistoryItem> HistoryGrid { get; set; }
    }

    public class YearEndConfigValueItem
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string FpsYearType { get; set; } = string.Empty;
    }

    public class YearEndMonthWorkingItem
    {
        public short Year { get; set; }
        public short Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal? Days { get; set; }
        public decimal? CvlHours { get; set; }
        public decimal? VidHours { get; set; }
        public short? Fmonth { get; set; }
        public int FpsYear { get; set; }
        public string FpsYearType { get; set; } = string.Empty;
    }

    public class YearEndHistoryItem
    {
        [Display(Name = "Job Name")]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.Text)]
        public string JobName { get; set; } = null!;

        [Display(Name = "Requested By")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text)]
        public string RequestedBy { get; set; } = null!;

        [Display(Name = "Start Date/Time")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Text)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Date/Time")]
        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.Text)]
        public DateTime? EndDateTime { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text)]
        public string Status { get; set; } = null!;

        [Display(Name = "Error Message")]
        [GridColumn(Order = 6, Width = 300, Type = GridColumnType.Text)]
        public string? ErrorMessage { get; set; }
    }
}
