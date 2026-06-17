namespace Apha.PACT.Core.Entities
{
    public class SubContractRmsImportRow
    {
        public int Id { get; set; }
        public string? Project { get; set; }
        public string? TestJob { get; set; }
        public double? Month { get; set; }
        public decimal? Amount { get; set; }
        public string? WorkGroup { get; set; }
        public string? AcctCode { get; set; }
        public string? Supplier { get; set; }
        public string? Description { get; set; }
        public int? SupplierNumber { get; set; }
        public decimal? DailyRate { get; set; }
        public int? AnimalDays { get; set; }
        public string? ValidationFailure { get; set; }
        public DateTime? ImportedDate { get; set; }
    }

    public class SubContractRmsImport
    {
        public string? FileName { get; set; }
        public List<SubContractRmsImportRow> Rows { get; set; } = new();
    }

    public class SubContractRmsImportResult
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
