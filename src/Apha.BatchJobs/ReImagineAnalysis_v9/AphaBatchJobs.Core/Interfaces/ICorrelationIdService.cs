namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service interface for generating unique correlation identifiers.
/// Correlation IDs are used to track job executions across the system.
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Generates a new unique identifier for correlation tracking.
    /// </summary>
    /// <returns>A unique string identifier for correlation purposes.</returns>
    string NewId();
}


// Review Comments:
// 1. The interface is well-structured and follows .NET 8 conventions
// 2. XML documentation is clear and properly formatted
// 3. The interface follows the Interface Segregation Principle (ISP) with a single, focused responsibility
// 4. Naming convention follows .NET standards (PascalCase for interface and method names)
// 5. The return type 'string' is appropriate for correlation IDs which are typically used in logging and tracing
// 6. No changes required - the code is already idiomatic and follows best practices
// 7. The interface is simple, testable, and maintainable
// 8. Consider: If this will be used with AWS X-Ray or distributed tracing, ensure implementations
//    generate IDs compatible with those systems (e.g., X-Ray trace ID format)