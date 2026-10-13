namespace Apha.FPSApps.Application.Dtos.CostBook;

public class BulkDeleteAnimalRequirementRequestDto
{
    public string ProjectId { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<AnimalRequirementBulkDeleteItemDto> Items { get; set; } = [];
}

public class AnimalRequirementBulkDeleteItemDto
{
    public int ArIdentity { get; set; }
}
