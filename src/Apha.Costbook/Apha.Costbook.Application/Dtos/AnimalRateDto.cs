namespace Apha.Costbook.Application.Dtos;

public class AnimalRateDto
{
    public string AnimalType { get; set; } = null!;
    public decimal? DailyRate { get; set; }
    public decimal? DailyRateWithInflamation { get; set; }
}
