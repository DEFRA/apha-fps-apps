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

public class TestCodeLookupDto
{
    public string ItemCode { get; set; } = null!;
    public string? ItemDescription { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class AnimalLookupDto
{
    public string AnimalType { get; set; } = null!;
    public string? Species { get; set; }
    public string? SecurityLevel { get; set; }
    public decimal? DailyRate { get; set; }
    public bool PlanByWeek { get; set; }
    public decimal? DefraDailyRate { get; set; }
}
