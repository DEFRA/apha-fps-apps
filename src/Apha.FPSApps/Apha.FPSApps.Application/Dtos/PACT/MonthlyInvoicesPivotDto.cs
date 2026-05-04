using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyInvoicesPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryRowDto> Rows { get; set; } = [];
        public PaginationDto Pagination { get; set; } = new();
    }
}
