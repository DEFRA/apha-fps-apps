namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Web-side DTO matching the JSON shape returned by the FPS API Bulk Rates endpoints
    /// (create, release, approve, reject, cancel, get). Mirrors <c>BulkRatesRequestDto</c>
    /// as serialised by the FPS API action filter into <c>ApiResponse&lt;T&gt;.Data</c>.
    /// </summary>
    public class BulkRatesRequestDetailDto
    {
        public BulkRatesQueueEntryDto Entry { get; set; } = new();
        public BulkRatesUploadMetadataDto? UploadMetadata { get; set; }
        public List<BulkRatesQueueLogDto> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}
