namespace Apha.Costbook.Application.Dtos;

public class BulkDeleteTestRequirementRequestDto
{
    public string ProjectId { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<TestRequirementBulkDeleteItemDto> Items { get; set; } = [];
}

public class TestRequirementBulkDeleteItemDto
{
    public string TestCode { get; set; } = string.Empty;
}
