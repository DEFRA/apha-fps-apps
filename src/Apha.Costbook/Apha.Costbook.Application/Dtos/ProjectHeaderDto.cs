namespace Apha.Costbook.Application.Dtos;

public class ProjectHeaderDto
{
    public string ProjectId { get; set; } = null!;
    public string? ProjectTitle { get; set; }
    public string? Programme { get; set; }
    public DateTime? StartDate { get; set; }
    public double? StartFYear { get; set; }
    public int? Inflation { get; set; }
    public int? FinancialYears { get; set; }
    public double? EuroConvRate { get; set; }
    public short? IsDefraProject { get; set; }
}
