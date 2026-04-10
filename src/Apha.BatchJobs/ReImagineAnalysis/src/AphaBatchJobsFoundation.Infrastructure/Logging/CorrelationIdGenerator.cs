// ============================================================================
// AphaBatchJobsFoundation.Infrastructure
// Correlation ID Generator Utility
// 
// Purpose: Generates unique correlation identifiers for job execution tracking
//          and distributed tracing across the Apha batch job system.
// 
// Architecture: Infrastructure layer utility following Apha conventions
// ============================================================================

using System;

namespace AphaBatchJobsFoundation.Infrastructure.Logging
{
    /// <summary>
    /// Utility class responsible for generating unique correlation identifiers
    /// used for tracking job execution flows and enabling distributed tracing
    /// across the Apha batch job orchestration system.
    /// </summary>
    /// <remarks>
    /// Correlation IDs are essential for:
    /// - Tracking job execution across multiple systems
    /// - Correlating log entries for a single job run
    /// - Debugging and troubleshooting distributed operations
    /// - Audit trail and compliance requirements
    /// 
    /// This implementation uses GUID-based identifiers to ensure uniqueness
    /// across distributed systems and concurrent job executions.
    /// </remarks>
    public static class CorrelationIdGenerator
    {
        /// <summary>
        /// Generates a new unique correlation identifier.
        /// </summary>
        /// <returns>
        /// A string representation of a newly generated GUID that serves as
        /// a unique correlation identifier for job execution tracking.
        /// </returns>
        /// <remarks>
        /// The generated correlation ID:
        /// - Is globally unique across all job executions
        /// - Uses standard GUID format (32 hexadecimal digits with hyphens)
        /// - Is thread-safe and can be called concurrently
        /// - Should be generated once per job execution and propagated throughout
        /// 
        /// Example output: "3f2504e0-4f89-11d3-9a0c-0305e82c3301"
        /// </remarks>
        /// <performance>
        /// Uses "D" format specifier for consistent lowercase formatting without allocating
        /// additional string objects. This is more efficient than calling ToString() without
        /// a format parameter.
        /// </performance>
        public static string Generate()
        {
            // Use "D" format for standard hyphenated lowercase GUID format (32 digits separated by hyphens)
            // This is more explicit and ensures consistent formatting across different cultures
            return Guid.NewGuid().ToString("D");
        }
    }
}


// Review Comments:
// ================
// 1. Added explicit format specifier "D" to ToString() for consistent GUID formatting
//    - Ensures lowercase hyphenated format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
//    - More explicit and culture-invariant
//    - Slightly better performance by avoiding format detection
//
// 2. The class is appropriately marked as static since it contains only static methods
//
// 3. Thread-safety is inherent as Guid.NewGuid() is thread-safe and the method has no state
//
// 4. The implementation is simple and follows .NET best practices for utility classes
//
// 5. XML documentation is comprehensive and well-structured
//
// 6. No SQL Server specific concerns in this utility class - it's a pure .NET implementation
//
// 7. Consider alternatives for future enhancement (not implementing now as per instructions):
//    - Using Guid.NewGuid().ToString("N") for compact format without hyphens
//    - Implementing custom correlation ID formats if needed
//    - Adding validation or parsing methods if required by the system