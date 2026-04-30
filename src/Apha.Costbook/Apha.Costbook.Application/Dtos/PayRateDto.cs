namespace Apha.Costbook.Application.Dtos;

public class PayRateDto
{
    public string WgGrade { get; set; } = null!;
    public double? ChargeRate { get; set; }
    public double? PayRate { get; set; }
    public double? Npr { get; set; }
    public double? Ohr { get; set; }
}
