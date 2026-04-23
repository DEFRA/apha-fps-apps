using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class TestRequirementItem
{
    [Display(Name = "Code")]
    [Required(ErrorMessage = "Test Code is required.")]
    [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
    public string TestCode { get; set; } = null!;

    [Display(Name = "Unit Price")]
    [GridColumn(Order = 2, Width = 100, Type = GridColumnType.GbpValue)]
    public double? UnitPrice { get; set; }

    [Display(Name = "No")]
    [GridColumn(Order = 3, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? NumberOfTests { get; set; }

    [Display(Name = "Cost")]
    [GridColumn(Order = 4, Width = 100, Type = GridColumnType.GbpValue)]
    public double? TestCost { get; set; }

    [GridColumn(IsVisible = false)]
    public string? TestDescription { get; set; }
}
