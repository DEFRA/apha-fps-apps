namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a single Bulk Rates staging validation finding.
    /// </summary>
    public class BulkRatesValidationErrorRes
    {
        public long Id { get; set; }
        public Guid JobQueueId { get; set; }
        public int UploadVersion { get; set; }
        public int SourceRowNumber { get; set; }
        public string? FieldName { get; set; }
        public string? ValidationCode { get; set; }
        public string Severity { get; set; } = "Error";
        public string ValidationMessage { get; set; } = string.Empty;
        public string? SheetName { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        public string? CurrentValue { get; set; }
        public string? ExpectedValue { get; set; }
        public bool IsRequestLevel { get; set; }
    }
}
