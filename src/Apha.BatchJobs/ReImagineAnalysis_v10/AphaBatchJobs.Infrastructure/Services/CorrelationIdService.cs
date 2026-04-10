namespace AphaBatchJobs.Infrastructure.Services;

using AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service implementation for generating unique correlation identifiers using GUID.
/// </summary>
/// <remarks>
/// This implementation uses System.Guid to generate globally unique identifiers.
/// The NewId method is thread-safe as Guid.NewGuid() is thread-safe by design.
/// Each call produces a new RFC 4122 compliant UUID v4 identifier.
/// </remarks>
public sealed class CorrelationIdService : ICorrelationIdService
{
    /// <summary>
    /// Generates a new unique identifier for correlating job execution logs and tracking.
    /// </summary>
    /// <returns>A unique string identifier in GUID format.</returns>
    /// <remarks>
    /// This method is thread-safe and can be called concurrently from multiple threads.
    /// The returned string is a lowercase GUID representation without braces or hyphens for better performance.
    /// Example format: "a1b2c3d4e5f678901234567890abcdef"
    /// </remarks>
    public string NewId()
    {
        // Use "N" format specifier for better performance (no hyphens, no braces, lowercase)
        // This reduces string allocation size from 36 to 32 characters
        // and avoids unnecessary formatting overhead
        return Guid.NewGuid().ToString("N");
    }
}
