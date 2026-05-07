namespace Apha.Common.Contracts.Costbook;

public class TestRequirementReq
{
    public string Project { get; set; } = null!;
    public int Year { get; set; }
    public string TestCode { get; set; } = null!;
    public double? NumberOfTests { get; set; }
    public double? UnitPrice { get; set; }
    public double? TestCost { get; set; }
}
