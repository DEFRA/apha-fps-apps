using Apha.Common.Contracts;

namespace Apha.Common.Contracts.PACT
{
    public class MonthlySubContractsPivotRes
    {
        public List<int> Months { get; set; } = [];
        public List<MonthlySubContractsSummaryItemRes> Rows { get; set; } = [];
        public Pagination Pagination { get; set; } = new();
    }

    public class MonthlySubContractsSummaryItemRes
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }
}
