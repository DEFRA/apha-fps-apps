using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class AnimalRequirementItem
{
    [GridColumn(IsVisible = false)]
    public int ArIdentity { get; set; }

    [Display(Name = "Animal Type")]
    [Required(ErrorMessage = "Animal Type is required.")]
    [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text, IsFilterable = false)]
    public string AnimalType { get; set; } = null!;

    [Display(Name = "Rate")]
    [Required(ErrorMessage = "Daily Rate is required.")]
    [GridColumn(Order = 2, Width = 100, Type = GridColumnType.GbpValue)]
    public double? DailyRate { get; set; }

    [Display(Name = "No")]
    [GridColumn(Order = 3, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? NumberOfAnimals { get; set; }

    [Display(Name = "Days")]
    [GridColumn(Order = 4, Width = 70, Type = GridColumnType.DecimalNumber)]
    public double? NumberOfDays { get; set; }

    [Display(Name = "Cost")]
    [Required(ErrorMessage = "Animal Cost is required.")]
    [GridColumn(Order = 5, Width = 100, Type = GridColumnType.GbpValue)]
    public double? AnimalCost { get; set; }
}
