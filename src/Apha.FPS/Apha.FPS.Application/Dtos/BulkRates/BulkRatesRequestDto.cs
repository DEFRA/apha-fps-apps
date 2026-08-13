namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract DTO combining the queue entry with parsed upload metadata.
    /// Returned by IBulkRatesRequestService.GetRequestAsync. Every nested property is a
    /// dedicated Dto type (not a Core entity) so the API contract can evolve independently
    /// of the persistence model.
    /// </summary>
    public class BulkRatesRequestDto
    {
        public BulkRatesQueueEntryDto Entry { get; set; } = null!;
        public BulkRatesUploadMetadataDto? UploadMetadata { get; set; }
        public IReadOnlyList<BulkRatesQueueLogDto> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}
