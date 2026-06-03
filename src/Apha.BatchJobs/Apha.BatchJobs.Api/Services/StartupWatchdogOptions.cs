namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Configures startup watchdog projection behavior for trigger-to-running monitoring.
/// </summary>
public sealed class StartupWatchdogOptions
{
    /// <summary>
    /// Startup SLA in seconds for Production.
    /// </summary>
    public int StartupSlaSecondsProduction { get; init; } = 600;

    /// <summary>
    /// Startup SLA in seconds for non-Production environments.
    /// </summary>
    public int StartupSlaSecondsNonProduction { get; init; } = 180;

    /// <summary>
    /// Startup polling minimum interval in seconds (for UI guidance).
    /// </summary>
    public int StartupPollMinSeconds { get; init; } = 2;

    /// <summary>
    /// Startup polling maximum interval in seconds (for UI guidance).
    /// </summary>
    public int StartupPollMaxSeconds { get; init; } = 5;

    /// <summary>
    /// Running-phase polling interval in seconds (for UI guidance).
    /// </summary>
    public int RunningPollSeconds { get; init; } = 15;
}
