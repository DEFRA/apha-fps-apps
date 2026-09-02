namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Mirrors <c>BulkRatesQueueLogDto</c> (Apha.FPS.Application) as serialised over the wire.
    /// </summary>
    public class BulkRatesQueueLogDto
    {
        public long LogId { get; set; }
        public Guid JobQueueId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
