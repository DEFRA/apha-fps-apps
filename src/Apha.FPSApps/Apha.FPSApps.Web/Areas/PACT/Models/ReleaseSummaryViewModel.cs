using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ReleaseSummaryViewModel
    {
        public required DataGridConfig<ReleasePeriodItem> ReleaseSummaryGrid { get; set; }
        public string? Setting { get; set; }
    }

    public class ReleasePeriodItem
    {
        [Display(Name = "Period Name")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text)]
        public string PeriodName { get; set; } = null!;

        [Display(Name = "Start Period")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text)]
        public double? StartPeriod { get; set; }

        [Display(Name = "End Period")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text)]
        public double? EndPeriod { get; set; }

        [Display(Name = "Final Summaries Run")]
        [GridColumn(Order = 4, Width = 160, Type = GridColumnType.Checkbox)]
        public short? FinalSummariesRun { get; set; }

        // Additional properties for JSON API response
        public string Period { get; set; } = null!;
        public string MonthNumber { get; set; } = null!;
        public string? PeriodType { get; set; }
    }
}