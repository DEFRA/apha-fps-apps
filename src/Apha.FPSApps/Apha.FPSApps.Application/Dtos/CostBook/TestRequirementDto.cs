namespace Apha.FPSApps.Application.Dtos.CostBook;

public class TestRequirementDto
{
    public string Project { get; set; } = null!;
    public int Year { get; set; }
    public string TestCode { get; set; } = null!;
    public double? NumberOfTests { get; set; }
    public double? UnitPrice { get; set; }
    public double? TestCost { get; set; }
    public string? TestDescription { get; set; }
}
