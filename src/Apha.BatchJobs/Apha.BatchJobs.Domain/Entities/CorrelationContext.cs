namespace Apha.BatchJobs.Domain.Entities;

/// <summary>Correlation metadata captured for a batch job execution flow.</summary>
public sealed class CorrelationContext
{
    /// <summary>Unique correlation identifier.</summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the context was created.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Logical job name associated with this context.</summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>Initializes an empty correlation context.</summary>
    public CorrelationContext()
    {
    }

    /// <summary>Initializes a correlation context with explicit values.</summary>
    /// <param name="correlationId">Correlation ID.</param>
    /// <param name="jobName">Job name.</param>
    public CorrelationContext(string correlationId, string jobName)
    {
        CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        JobName = jobName ?? throw new ArgumentNullException(nameof(jobName));
        Timestamp = DateTime.UtcNow;
    }
}
