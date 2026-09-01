namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Web-side DTO matching the JSON shape returned by the FPS API upload and
    /// validation endpoints (<c>POST /upload</c>, <c>GET /validation</c>).
    /// Mirrors <c>BulkRatesUploadResultDto</c> as serialised by the FPS API.
    /// </summary>
    public class BulkRatesUploadResultDto
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
        public List<BulkRatesValidationErrorDto> ValidationErrors { get; set; } = [];
    }
}
