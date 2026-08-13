namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract DTO returned by the Upload operation to the controller.
    /// Carries the new validation state so the controller can build the upload response.
    /// Every nested property is a dedicated Dto type (not a Core entity) so the API
    /// contract can evolve independently of the persistence model.
    /// </summary>
    public class BulkRatesUploadResultDto
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
        public IReadOnlyList<BulkRatesValidationErrorDto> ValidationErrors { get; set; } = [];
    }
}
