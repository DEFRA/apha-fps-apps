namespace Apha.Costbook.Application.Dtos;

public class PayRateDto
{
    public string WgGrade { get; set; } = null!;
    public decimal? ChargeRate { get; set; }
    public decimal? PayRate { get; set; }
    public decimal? Npr { get; set; }
    public decimal? Ohr { get; set; }
    public decimal? ChargeRateWithInflamation { get; set; }
}
