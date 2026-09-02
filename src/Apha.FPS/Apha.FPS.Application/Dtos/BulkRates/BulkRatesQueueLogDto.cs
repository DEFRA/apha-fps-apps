namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// API/JSON contract shape mirroring <see cref="Apha.FPS.Core.Entities.BatchJobQueueLog"/>
    /// (the shared EF entity for fps.job_queue_log, reused directly rather than duplicated —
    /// see BulkRatesRequestService.ToDto(BatchJobQueueLog)) with persistence-oriented names
    /// translated to consumer-friendly ones: JobqueueLogId -&gt; LogId, PerformedBy -&gt; Actor,
    /// LogTime -&gt; CreatedAtUtc.
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
