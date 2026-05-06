namespace Apha.Common.Contracts.PACT
{
    public class MonthlyInvoicesSummaryItemRes
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }
}
