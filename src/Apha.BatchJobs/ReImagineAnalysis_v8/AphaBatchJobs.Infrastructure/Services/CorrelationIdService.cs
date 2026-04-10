namespace AphaBatchJobs.Infrastructure.Services;

using AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service implementation that generates unique correlation identifiers using Guid for job execution tracking.
/// This service provides thread-safe generation of unique identifiers used to correlate log entries and operations
/// across the batch job execution lifecycle.
/// </summary>
public sealed class CorrelationIdService : ICorrelationIdService
{
    /// <summary>
    /// Generates and returns a new unique identifier as a string for correlation tracking.
    /// Uses Guid.NewGuid() to ensure uniqueness and thread-safety.
    /// The "N" format specifier produces a 32-character hexadecimal string without hyphens,
    /// which is more compact and efficient for logging and storage.
    /// </summary>
    /// <returns>A new unique identifier string in compact GUID format (without hyphens) for correlation tracking.</returns>
    public string NewId()
    {
        // Using "N" format for better performance and reduced string size (32 chars vs 36 chars)
        // This format is more suitable for correlation IDs in distributed systems and logging
        return Guid.NewGuid().ToString("N");
    }
}


// Key improvements made:
// 1. Changed ToString() to ToString("N") for more efficient string representation
//    - Removes hyphens, reducing string length from 36 to 32 characters
//    - Better for database storage, logging, and network transmission
//    - Still maintains uniqueness and readability
// 2. Added inline comment explaining the format choice
// 3. Updated XML documentation to reflect the format change
// 4. Maintained thread-safety (Guid.NewGuid() is inherently thread-safe)
// 5. Kept the sealed class modifier for performance optimization
// 6. No additional dependencies or complexity introduced