using Apha.Common.Contracts;

namespace Apha.Common.Contracts.PACT
{
    public class MonthlyInvoicesPivotRes
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryItemRes> Rows { get; set; } = [];
        public Pagination Pagination { get; set; } = new();
    }
}
