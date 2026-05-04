namespace Apha.Common.Contracts.Costbook;

public class AdditionalCostReq
{
    public int? AcIdentity { get; set; }
    public string Project { get; set; } = null!;
    public int Year { get; set; }
    public string AccountCat { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double CostEntered { get; set; }
    public double? ItemCost { get; set; }
    public string? Freq { get; set; }
}
