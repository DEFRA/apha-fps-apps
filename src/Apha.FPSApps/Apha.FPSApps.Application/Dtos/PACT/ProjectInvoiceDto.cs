namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ProjectInvoiceDto
    {
        public int InvoiceCounter { get; set; }
        public string ProjectParent { get; set; } = null!;
        public int? Month { get; set; }
        public decimal? Amount { get; set; }
        public decimal? CostOfWork { get; set; }
        public decimal? Wip { get; set; }
        public decimal? ProfitLoss { get; set; }
        public string? Detail { get; set; }
        public string? Type { get; set; }
    }
}
