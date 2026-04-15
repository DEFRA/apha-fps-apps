namespace Apha.PACT.Core.Entities
{
    public partial class ProjectInvoice
    {
        public string ProjectParent { get; set; } = null!;

        public int? Month { get; set; }

        public decimal? Amount { get; set; }

        public decimal? CostOfWork { get; set; }

        public decimal? Wip { get; set; }

        public decimal? ProfitLoss { get; set; }

        public string? Detail { get; set; }

        public int InvoiceCounter { get; set; }

        public string? X { get; set; }

        public string? Type { get; set; }

        public int FpsYear { get; set; }
    }
}