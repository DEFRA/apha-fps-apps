namespace Apha.Costbook.Application.Dtos;

public class AnimalRequirementDto
{
    public int ArIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string AnimalType { get; set; } = null!;
    public double? NumberOfDays { get; set; }
    public double? NumberOfAnimals { get; set; }
    public double? DailyRate { get; set; }
    public double? AnimalCost { get; set; }
}
