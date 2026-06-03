namespace Apha.Common.Contracts.Costbook;

public class AnimalRateRes
{
    public string AnimalType { get; set; } = null!;
    public decimal? DailyRate { get; set; }
    public decimal? DailyRateWithInflamation { get; set; }
}
