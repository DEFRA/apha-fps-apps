namespace Apha.Common.Contracts.PACT
{
    public class InvoiceImportRowReq
    {
        public int Id { get; set; }
        public string? ProjectParent { get; set; }
        public string? Month { get; set; }
        public string? Amount { get; set; }
        public string? CostOfWork { get; set; }
        public string? Wip { get; set; }
        public string? ProfitLoss { get; set; }
        public string? Detail { get; set; }
        public string? Type { get; set; }
        public string? ValidationFailure { get; set; }
        public DateTime? ImportedDate { get; set; }
    }

    public class InvoiceImportReq
    {
        public string? FileName { get; set; }
        public List<InvoiceImportRowReq> Rows { get; set; } = new();
    }
}