namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyInvoicesPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryRowDto> Rows { get; set; } = [];
    }
}
