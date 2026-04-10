namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service interface for generating unique correlation identifiers.
/// Correlation IDs are used to track and correlate log entries and operations
/// across the batch job execution lifecycle.
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Generates a new unique correlation identifier.
    /// </summary>
    /// <returns>A unique string identifier that can be used to correlate related operations and log entries.</returns>
    string NewId();
}


// Changes made:
// 1. Removed unnecessary braces around namespace declaration - using file-scoped namespace (C# 10/.NET 6+ feature)
// 2. This makes the code more concise and reduces indentation levels
// 3. File-scoped namespaces are the recommended style in .NET 10 and modern C# development
// 4. The interface remains functionally identical but follows modern C# conventions