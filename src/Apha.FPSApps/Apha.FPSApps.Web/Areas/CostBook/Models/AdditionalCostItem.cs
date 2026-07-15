using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class AdditionalCostItem
{
    [GridColumn(IsVisible = false)]
    public int AcIdentity { get; set; }

    [Display(Name = "Description")]
    [Required(ErrorMessage = "Description is required.")]
    [GridColumn(Order = 1, Width = 180, Type = GridColumnType.Text)]
    public string Description { get; set; } = null!;

    [Display(Name = "Cost No")]
    [GridColumn(Order = 2, Width = 110, Type = GridColumnType.GbpValue)]
    public double CostEntered { get; set; }

    [Display(Name = "Cost(inf)")]    
    [GridColumn(Order = 3, Width = 110, Type = GridColumnType.GbpValue)]
    public double? ItemCost { get; set; }

    [Display(Name = "Account Cat")]
    [Required(ErrorMessage = "Account Category is required.")]
    [GridColumn(Order = 4, Width = 130, Type = GridColumnType.Text, IsFilterable = false)]
    public string AccountCat { get; set; } = null!;
}
