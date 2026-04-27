namespace Apha.Common.Contracts.Costbook;

public class AddProjectYearReq
{
    public string Project { get; set; } = null!;
    public int Year { get; set; }
    public int YearValue { get; set; }
    public double? MarkupTime { get; set; }
    public double? MarkupTests { get; set; }
    public double? MarkupAnimals { get; set; }
    public double? MarkupAdditional { get; set; }
    public double? ProfitTime { get; set; }
    public double? ProfitTests { get; set; }
    public double? ProfitAnimals { get; set; }
    public double? ProfitAdditional { get; set; }
}
