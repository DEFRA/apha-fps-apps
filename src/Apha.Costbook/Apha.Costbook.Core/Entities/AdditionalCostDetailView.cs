namespace Apha.Costbook.Core.Entities;

/// <summary>
/// Read model for the enriched additional cost query
/// (equivalent of MS Access qryAdditionalCosts).
/// </summary>
public class AdditionalCostDetailView
{
    public int AcIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string Description { get; set; } = null!;
    public double? ItemCost { get; set; }
    public double CostEntered { get; set; }
    public string AccountCat { get; set; } = null!;
    public string? Freq { get; set; }

    // From Project (tblProject) join
    public string? Programme { get; set; }
    public double? EuroConvRate { get; set; }
}
