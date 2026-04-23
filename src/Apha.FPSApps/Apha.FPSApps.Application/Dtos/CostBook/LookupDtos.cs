namespace Apha.FPSApps.Application.Dtos.CostBook;

public class PayRateDto
{
    public string WgGrade { get; set; } = null!;
    public double? ChargeRate { get; set; }
    public double? PayRate { get; set; }
    public double? Npr { get; set; }
    public double? Ohr { get; set; }
}

public class AnimalRateDto
{
    public string AnimalType { get; set; } = null!;
    public double? DailyRate { get; set; }
}

public class AccountCategoryDto
{
    public string AccShortName { get; set; } = null!;
    public bool UseInflation { get; set; }
}
