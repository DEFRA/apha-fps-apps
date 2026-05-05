using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class ProjectYearRateItem
{
    [GridColumn(IsVisible = false)]
    public string Project { get; set; } = null!;

    [Display(Name = "Year")]
    [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Text)]
    public int YearValue { get; set; }

    [Display(Name = "Markup Time %")]
    [GridColumn(Order = 2, Width = 110, Type = GridColumnType.Number)]
    public double? MarkupTime { get; set; }

    [Display(Name = "Markup Tests %")]
    [GridColumn(Order = 3, Width = 110, Type = GridColumnType.Number)]
    public double? MarkupTests { get; set; }

    [Display(Name = "Markup Animals %")]
    [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Number)]
    public double? MarkupAnimals { get; set; }

    [Display(Name = "Markup Additional %")]
    [GridColumn(Order = 5, Width = 130, Type = GridColumnType.Number)]
    public double? MarkupAdditional { get; set; }

    [Display(Name = "Profit Time %")]
    [GridColumn(Order = 6, Width = 110, Type = GridColumnType.Number)]
    public double? ProfitTime { get; set; }

    [Display(Name = "Profit Tests %")]
    [GridColumn(Order = 7, Width = 110, Type = GridColumnType.Number)]
    public double? ProfitTests { get; set; }

    [Display(Name = "Profit Animals %")]
    [GridColumn(Order = 8, Width = 120, Type = GridColumnType.Number)]
    public double? ProfitAnimals { get; set; }

    [Display(Name = "Profit Additional %")]
    [GridColumn(Order = 9, Width = 130, Type = GridColumnType.Number)]
    public double? ProfitAdditional { get; set; }
    public string? Programme { get; set; }
}
