using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class TestRequirementItem
{
    [Display(Name = "Code")]
    [Required(ErrorMessage = "Test Code is required.")]
    [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
    public string TestCode { get; set; } = null!;

    [Display(Name = "Unit Price")]
    [Required(ErrorMessage = "Unit Price is required.")]
    [GridColumn(Order = 2, Width = 100, Type = GridColumnType.GbpValue)]
    public double? UnitPrice { get; set; }

    [Display(Name = "No")]
    [Required(ErrorMessage = "Number of Tests is required.")]
    [GridColumn(Order = 3, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? NumberOfTests { get; set; }

    [Display(Name = "Cost")]
    [Required(ErrorMessage = "Test Cost is required.")]
    [GridColumn(Order = 4, Width = 100, Type = GridColumnType.GbpValue)]
    public double? TestCost { get; set; }

    [GridColumn(IsVisible = false)]
    public string? TestDescription { get; set; }
}
