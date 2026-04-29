namespace Apha.BatchJobs.Application.Jobs.FECProcess;

/// <summary>
/// Request model for the FEC process batch job.
/// Foundation layer placeholder: structure defined for future use.
/// </summary>
public sealed class FECProcessRequest
{
    /// <summary>
    /// Processing mode (e.g., 'Validation', 'Reconciliation', 'Correction', 'Full').
    /// </summary>
    public string ProcessingMode { get; set; } = "Validation";

    /// <summary>
    /// Date or period identifier for FEC processing scope.
    /// </summary>
    public string? ProcessingPeriod { get; set; }

    /// <summary>
    /// Whether to perform a dry-run without persisting changes.
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Optional notification email for completion status.
    /// </summary>
    public string? NotificationEmail { get; set; }
}
