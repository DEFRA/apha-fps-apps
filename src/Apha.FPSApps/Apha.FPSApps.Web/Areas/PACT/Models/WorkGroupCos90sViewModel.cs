using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupCos90SViewModel
    {
        // ── Filter bar ──────────────────────────────────────────────────────
        public string? SelectedProfitCentre { get; set; }
        public string? SelectedIndividual { get; set; }
        public short? SelectedMonthNumber { get; set; }
        public short? SelectedYear { get; set; }

        // ── Dropdowns ────────────────────────────────────────────────────────
        public List<SelectListItem> ProfitCentreOptions { get; set; } = new();
        public List<SelectListItem> IndividualOptions { get; set; } = new();
        public List<CalenderMonthDto> CalenderMonthItems { get; set; } = new();
        public List<short> YearOptions { get; set; } = new();

        // ── Work-group grid (uses shared DataGrid component) ─────────────────
        public DataGridConfig<WorkGroupCos90SWorkGroupItem> WorkGroupGrid { get; set; } = new();

        // ── Flagged work groups (for COS90 generation) ───────────────────────
        public List<Cos90WorkGroupDto> FlaggedWorkGroups { get; set; } = new();
    }

    public class WorkGroupCos90SWorkGroupItem
    {
        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroupName { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public string? ProfitCentre { get; set; }

        [GridColumn(IsVisible = false)]
        public bool Cos90Flagged { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        [Display(Name = "COS90? Yes")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Cos90Yes => Cos90Flagged;

        [Display(Name = "COS90? No")]
        [GridColumn(Order = 3, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Cos90No => !Cos90Flagged;
    }

    public class MonthHourRowItem
    {
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Text, IsFilterable = false)]
        public short Year { get; set; }

        [Display(Name = "Month")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Text, IsFilterable = false)]
        public short Month { get; set; }

        [Display(Name = "Days")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.Text, IsFilterable = false)]
        public decimal? Days { get; set; }

        [Display(Name = "CVL Hours")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public decimal? CvlHours { get; set; }

        [Display(Name = "VID Hours")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public decimal? VidHours { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Fmonth { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
