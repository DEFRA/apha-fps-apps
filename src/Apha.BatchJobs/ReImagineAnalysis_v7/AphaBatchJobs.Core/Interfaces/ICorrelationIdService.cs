namespace AphaBatchJobs.Core.Interfaces;

/// <summary>
/// Service interface for generating unique correlation identifiers.
/// Correlation IDs are used to track and correlate job executions across the system.
/// </summary>
public interface ICorrelationIdService
{
    /// <summary>
    /// Generates a new unique identifier for correlation tracking.
    /// </summary>
    /// <returns>A unique string identifier that can be used to correlate job execution logs and events.</returns>
    string NewId();
}


// Review Comments:
// 1. The interface is well-defined and follows .NET naming conventions
// 2. XML documentation is comprehensive and clear
// 3. The interface follows the Interface Segregation Principle (ISP) with a single, focused responsibility
// 4. No changes required - the code is already idiomatic and follows .NET best practices
// 5. Consider: If this will be used in async contexts, you might want to add an async variant in the future,
//    but since the current implementation doesn't have it, no changes are made per the instructions
// 6. The return type 'string' is appropriate for correlation IDs (commonly used for distributed tracing)
// 7. Namespace follows proper convention for the project structure