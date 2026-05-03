namespace Apha.Common.Contracts.Costbook;

public class AnimalRequirementRes
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
