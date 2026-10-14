namespace Apha.FPSApps.Application.Dtos.CostBook;

public class BulkDeleteStaffRequirementRequestDto
{
    public string ProjectId { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<StaffRequirementBulkDeleteItemDto> Items { get; set; } = [];
}

public class StaffRequirementBulkDeleteItemDto
{
    public int SrIdentity { get; set; }
}
