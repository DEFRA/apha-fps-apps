namespace Apha.Costbook.Core.Entities;

public class AnimalRateLookup
{
    public string AnimalType { get; set; } = null!;
    public decimal? DailyRate { get; set; }

    public decimal? DailyRateWithInflamation { get; set; }
}
