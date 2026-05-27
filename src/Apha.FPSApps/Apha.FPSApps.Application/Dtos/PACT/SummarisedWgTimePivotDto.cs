using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Dtos.PACT;

public class SummarisedWgTimePivotDto
{
    public List<int> Months { get; set; } = [];
    public List<SummarisedWgTimeDto> Rows { get; set; } = [];
    public PaginationDto Pagination { get; set; } = new();
}
