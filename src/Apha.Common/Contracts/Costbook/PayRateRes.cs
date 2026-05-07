namespace Apha.Common.Contracts.Costbook;

public class PayRateRes
{
    public string WgGrade { get; set; } = null!;
    public double? ChargeRate { get; set; }
    public double? PayRate { get; set; }
    public double? Npr { get; set; }
    public double? Ohr { get; set; }
}
