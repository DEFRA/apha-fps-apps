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
    [Required(ErrorMessage = "Number of Days is required.")]
    public double? NumberOfDays { get; set; }
    [Required(ErrorMessage = "Number of Animals is required.")]
    public double? NumberOfAnimals { get; set; }
    [Required(ErrorMessage = "Daily Rate is required.")]
    public double? DailyRate { get; set; }
    [Required(ErrorMessage = "Animal Cost is required.")]
    public double? AnimalCost { get; set; }
}
