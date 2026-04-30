namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyInvoicesSummaryRowDto
    {
        public string Program { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = [];
    }
}
