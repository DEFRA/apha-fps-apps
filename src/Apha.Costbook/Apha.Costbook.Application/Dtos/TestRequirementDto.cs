using System.ComponentModel.DataAnnotations;

namespace Apha.Costbook.Application.Dtos;

public class TestRequirementDto
{
    [Required(ErrorMessage = "Project is required.")]
    public string Project { get; set; } = null!;
    [Required(ErrorMessage = "Year is required.")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Test Code is required.")]
    public string TestCode { get; set; } = null!;
    [Required(ErrorMessage = "Number of Tests is required.")]
    public double? NumberOfTests { get; set; }
    [Required(ErrorMessage = "Unit Price is required.")]
    public double? UnitPrice { get; set; }
    [Required(ErrorMessage = "Test Cost is required.")]
    public double? TestCost { get; set; }
    public string? TestDescription { get; set; }
}
