namespace Apha.FPSApps.Application.Dtos.CostBook;

public class AdditionalCostDto
{
    public int AcIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string AccountCat { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double? ItemCost { get; set; }
    public double CostEntered { get; set; }
    public string? Freq { get; set; }
}
