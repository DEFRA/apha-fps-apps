using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlySubContractsPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlySubContractsSummaryItemDto> Rows { get; set; } = [];
        public PaginationDto Pagination { get; set; } = new();
    }
}
