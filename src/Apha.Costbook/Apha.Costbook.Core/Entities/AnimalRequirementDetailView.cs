namespace Apha.Costbook.Core.Entities;

/// <summary>
/// Read model for the enriched animal requirement query
/// (equivalent of MS Access qryAnimalReq).
/// </summary>
public class AnimalRequirementDetailView
{
    public int ArIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string AnimalType { get; set; } = null!;
    public double? NumberOfDays { get; set; }
    public double? NumberOfAnimals { get; set; }
    public double? DailyRate { get; set; }
    public double? AnimalCost { get; set; }

    // From Project (tblProject) join
    public string? Programme { get; set; }
    public double? EuroConvRate { get; set; }
}
