using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models;

public class SummarisedWgTimePivotRow
{
    [Display(Name = "Project")]
    [GridColumn(Order = 1, Width = 130, Type = GridColumnType.Text)]
    public string? ParentProject { get; set; }

    [Display(Name = "Apr")]
    [GridColumn(Order = 2, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal April { get; set; }

    [Display(Name = "May")]
    [GridColumn(Order = 3, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal May { get; set; }

    [Display(Name = "Jun")]
    [GridColumn(Order = 4, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal June { get; set; }

    [Display(Name = "Jul")]
    [GridColumn(Order = 5, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal July { get; set; }

    [Display(Name = "Aug")]
    [GridColumn(Order = 6, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal August { get; set; }

    [Display(Name = "Sep")]
    [GridColumn(Order = 7, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal September { get; set; }

    [Display(Name = "Oct")]
    [GridColumn(Order = 8, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal October { get; set; }

    [Display(Name = "Nov")]
    [GridColumn(Order = 9, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal November { get; set; }

    [Display(Name = "Dec")]
    [GridColumn(Order = 10, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal December { get; set; }

    [Display(Name = "Jan")]
    [GridColumn(Order = 11, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal January { get; set; }

    [Display(Name = "Feb")]
    [GridColumn(Order = 12, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal February { get; set; }

    [Display(Name = "Mar")]
    [GridColumn(Order = 13, Width = 55, Type = GridColumnType.DecimalNumber)]
    public decimal March { get; set; }

    [Display(Name = "Time")]
    [GridColumn(Order = 14, Width = 130, Type = GridColumnType.Text)]
    public decimal SumOfTime { get; set; }

    // Used for the YrPlan % calculation – hidden from the grid
    [GridColumn(IsVisible = false)]
    public decimal SumOfCost { get; set; }

    [Display(Name = "Cost")]
    [GridColumn(Order = 15, Width = 130, Type = GridColumnType.Text)]
    public string CostDisplay { get; set; } = string.Empty;

    [Display(Name = "YrPlan")]
    [GridColumn(Order = 16, Width = 130, Type = GridColumnType.Text)]
    public decimal? Budget { get; set; }

    // Used for calculation only – hidden from the grid
    [GridColumn(IsVisible = false)]
    public decimal? PercentSpent { get; set; }

    [Display(Name = "Spent")]
    [GridColumn(Order = 17, Width = 130, Type = GridColumnType.Text)]
    public string SpentDisplay { get; set; } = string.Empty;
}
