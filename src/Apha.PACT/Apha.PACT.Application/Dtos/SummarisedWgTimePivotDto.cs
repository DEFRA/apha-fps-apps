using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos;
public class SummarisedWgTimePivotDto
{
    public List<int> Months { get; set; } = [];
    public List<SummarisedWgTimeDto> Rows { get; set; } = [];
    public PaginationDto Pagination { get; set; } = new();
}
