using System;

namespace Apha.Costbook.Core.Entities;

public partial class PerformanceLog
{
    public long Id { get; set; }

    public string LogType { get; set; } = null!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public long DurationMs { get; set; }

    public string? CommandKind { get; set; }

    public string? CommandId { get; set; }

    public string? CommandText { get; set; }

    public string? Parameters { get; set; }

    public string? ApiName { get; set; }

    public string? HttpMethod { get; set; }

    public string? StatusCode { get; set; }

    public string? Outcome { get; set; }

    public string? Url { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; }
}
