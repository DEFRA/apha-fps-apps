using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class StaffRequirementItem
{
    [GridColumn(IsVisible = false)]
    public int SrIdentity { get; set; }

    [Display(Name = "WG Grade")]
    [Required(ErrorMessage = "WG Grade is required.")]
    [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = false)]
    public string WgGrade { get; set; } = null!;

    [Display(Name = "Name")]
    [Required(ErrorMessage = "Name is required.")]
    [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text)]
    public string? Name { get; set; }

    [Display(Name = "Rate")]
    [Required(ErrorMessage = "Rate is required.")]
    [GridColumn(Order = 3, Width = 100, Type = GridColumnType.GbpValue)]
    public double? Chargerate { get; set; }

    [Display(Name = "Hrs")]
    [Required(ErrorMessage = "Hrs is required.")]
    [GridColumn(Order = 4, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? Nohours { get; set; }

    [Display(Name = "Days")]
    [Required(ErrorMessage = "Days is required.")]
    [GridColumn(Order = 5, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? Nodays { get; set; }

    [Display(Name = "Cost")]
    [Required(ErrorMessage = "Cost is required.")]
    [GridColumn(Order = 6, Width = 100, Type = GridColumnType.GbpValue)]
    public double? StaffCost { get; set; }

    [GridColumn(IsVisible = false)]
    public double? Payrate { get; set; }

    [GridColumn(IsVisible = false)]
    public double? Npr { get; set; }

    [GridColumn(IsVisible = false)]
    public double? Ohr { get; set; }
}
