using Apha.Common.Contracts;

namespace Apha.Common.Contracts.PACT
{
    public class MonthlyInvoicesSummaryRowRes
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }

    public class MonthlyInvoicesPivotRes
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlyInvoicesSummaryRowRes> Rows { get; set; } = [];
        public Pagination Pagination { get; set; } = new();
    }
}
