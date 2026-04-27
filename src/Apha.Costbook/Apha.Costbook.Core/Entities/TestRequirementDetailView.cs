namespace Apha.Costbook.Core.Entities;

/// <summary>
/// Read model for the enriched test requirement query
/// (equivalent of MS Access qryTestReqTest).
/// </summary>
public class TestRequirementDetailView
{
    public string Project { get; set; } = null!;
    public int Year { get; set; }
    public string TestCode { get; set; } = null!;
    public double? UnitPrice { get; set; }
    public double? NumberOfTests { get; set; }
    public double? TestCost { get; set; }

    // From FpsTestOrProduct (tblTest) join
    public string? TestDescription { get; set; }

    // From Project (tblProject) join
    public string? Programme { get; set; }
    public double? EuroConvRate { get; set; }
}
