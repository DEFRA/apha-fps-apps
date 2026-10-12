namespace Apha.FPSApps.Application.Dtos.CostBook;

public class BulkDeleteAdditionalCostRequestDto
{
    public string ProjectId { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<AdditionalCostBulkDeleteItemDto> Items { get; set; } = [];
}

public class AdditionalCostBulkDeleteItemDto
{
    public int AcIdentity { get; set; }
}
