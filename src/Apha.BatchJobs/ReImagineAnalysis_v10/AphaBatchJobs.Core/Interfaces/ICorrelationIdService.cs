namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service interface for generating unique correlation identifiers for tracking job executions.
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Generates a new unique identifier for correlating job execution logs and tracking.
    /// </summary>
    /// <returns>A unique string identifier.</returns>
    /// <remarks>
    /// Implementations should ensure thread-safety and generate globally unique identifiers.
    /// Common implementations use GUID, ULID, or distributed ID generation strategies.
    /// </remarks>
    string NewId();
}
