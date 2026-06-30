using System.ComponentModel.DataAnnotations;

namespace Apha.Costbook.Application.Dtos;

public class AnimalRequirementDto
{
    public int ArIdentity { get; set; }
    [Required(ErrorMessage = "Project is required.")]
    public string? Project { get; set; }
    [Required(ErrorMessage = "Year is required.")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Animal Type is required.")]
    public string AnimalType { get; set; } = null!;    
    public double? NumberOfDays { get; set; }    
    public double? NumberOfAnimals { get; set; }
    
    public double? DailyRate { get; set; }   
    public double? AnimalCost { get; set; }
}
