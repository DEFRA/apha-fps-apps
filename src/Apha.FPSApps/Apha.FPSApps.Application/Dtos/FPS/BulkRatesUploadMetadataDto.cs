namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Mirrors <c>BulkRatesUploadMetadata</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesUploadMetadataDto
    {
        public string? Filename { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
    }
}
