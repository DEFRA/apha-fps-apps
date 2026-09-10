namespace Apha.Common.Contracts
{
    /// <summary>
    /// Request payload used to persist a performance-log entry (query execution or
    /// outbound API call) into the centralised <c>performance_log</c> table.
    /// </summary>
    public class PerformanceLogReq
    {
        /// <summary>Discriminator: "Query" or "Api".</summary>
        public string LogType { get; set; } = null!;

        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }

        public long DurationMs { get; set; }

        // Query-specific
        public string? CommandKind { get; set; }
        public string? CommandId { get; set; }
        public string? CommandText { get; set; }
        public string? Parameters { get; set; }

        // Api-specific
        public string? ApiName { get; set; }
        public string? HttpMethod { get; set; }
        public string? StatusCode { get; set; }
        public string? Outcome { get; set; }
        public string? Url { get; set; }
        public string? CorrelationId { get; set; }
    }
}
