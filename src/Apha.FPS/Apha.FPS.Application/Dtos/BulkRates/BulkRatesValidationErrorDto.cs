namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract shape mirroring <see cref="Apha.FPS.Core.Entities.StagingValidationError"/>
    /// exactly, including <see cref="Id"/>/<see cref="JobQueueId"/>/<see cref="UploadVersion"/>
    /// (wire-preserving — a boundary correction, not a contract trim; whether those three fields
    /// are actually needed on the wire is a separate, later API-contract decision).
    /// </summary>
    public class BulkRatesValidationErrorDto
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
