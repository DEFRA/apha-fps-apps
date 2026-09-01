namespace Apha.PACT.Core.Entities
{
    public class InvoiceImportRow
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

    public class InvoiceImport
    {
        public string? FileName { get; set; }
        public List<InvoiceImportRow> Rows { get; set; } = new();
    }

    public class InvoiceImportResult
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
