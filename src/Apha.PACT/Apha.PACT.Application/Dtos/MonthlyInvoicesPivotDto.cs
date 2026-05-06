using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Dtos
{
    public class MonthlyInvoicesPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryDto> Rows { get; set; } = [];
        public PaginationDto Pagination { get; set; } = new();
    }
}
