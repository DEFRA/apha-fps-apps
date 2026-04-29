namespace Apha.PACT.Application.Dtos
{
    public class MonthlyInvoicesSummaryDto
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }

    public class MonthlyInvoicesPivotDto
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryDto> Rows { get; set; } = [];
    }
}
