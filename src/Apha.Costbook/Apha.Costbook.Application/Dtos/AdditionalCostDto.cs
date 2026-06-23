using System.ComponentModel.DataAnnotations;

namespace Apha.Costbook.Application.Dtos;

public class AdditionalCostDto
{
    public int AcIdentity { get; set; }
    [Required(ErrorMessage = "Project is required.")]
    public string? Project { get; set; }
    [Required(ErrorMessage = "Year is required.")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Account Category is required.")]
    public string AccountCat { get; set; } = null!;
    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = null!;
    public double? ItemCost { get; set; }    
    public double CostEntered { get; set; }
    public string? Freq { get; set; }
}
