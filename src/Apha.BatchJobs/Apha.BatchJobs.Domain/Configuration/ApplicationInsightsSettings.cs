namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Application Insights configuration settings.
/// </summary>
public sealed class ApplicationInsightsSettings
{
    /// <summary>
    /// Whether Application Insights logging is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Application Insights instrumentation key.
    /// </summary>
    public string? InstrumentationKey { get; set; }
}
