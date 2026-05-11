using System;

namespace AphaBatchJobs.Core.Interfaces
{
    /// <summary>
    /// Interface for correlation ID service used for tracking and logging job execution 
    /// across distributed operations in the AphaBatchJobs system.
    /// Provides correlation ID management for tracing execution flow across multiple steps,
    /// services, and log entries within scheduled and adhoc job executions.
    /// </summary>
    /// <remarks>
    /// This service is essential for:
    /// - Tracking job execution across the 5-step orchestration in ScheduledLoadFromFpsJob
    /// - Correlating log entries from different components during a single job run
    /// - Debugging and troubleshooting distributed job execution flows
    /// - Maintaining audit trails for compliance and operational monitoring
    /// 
    /// Implementation should be registered as Scoped in DI container to ensure
    /// a unique correlation ID per job execution context.
    /// </remarks>
    public interface ICorrelationIdService
    {
        /// <summary>
        /// Retrieves the current correlation ID for the active execution context.
        /// This ID is used for logging and tracking purposes across all step executions
        /// within a job run, enabling correlation of log entries and tracking of execution flow.
        /// </summary>
        /// <returns>
        /// A string representing the unique correlation ID for the current execution context.
        /// The correlation ID should be consistent throughout a single job execution lifecycle
        /// and should be unique across different job executions.
        /// Returns null if no correlation ID has been set for the current context.
        /// </returns>
        /// <remarks>
        /// The correlation ID is typically:
        /// - Generated once per job execution (at job start)
        /// - Maintained throughout all 5 steps of the orchestration
        /// - Included in all log entries for that execution
        /// - Used to correlate distributed operations and async tasks
        /// - Formatted as a GUID or similar unique identifier
        /// 
        /// Usage example in ScheduledLoadFromFpsJob:
        /// <code>
        /// var correlationId = _correlationIdService.GetCorrelationId();
        /// _logger.LogInformation("Step 1 starting - CorrelationId: {CorrelationId}", correlationId);
        /// </code>
        /// </remarks>
        string? GetCorrelationId();

        /// <summary>
        /// Sets the correlation ID for the current execution context.
        /// This method should be called at the beginning of a job execution to establish
        /// a unique identifier for tracking the entire execution flow.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set for the current context.</param>
        /// <remarks>
        /// This method is typically called once at the start of job execution.
        /// Subsequent calls within the same scope will override the previous value.
        /// </remarks>
        void SetCorrelationId(string correlationId);
    }
}


**Review Comments:**

1. **Nullable Reference Type**: Changed return type from `string` to `string?` to properly indicate that the method may return null if no correlation ID has been set, aligning with .NET 8 nullable reference types best practices.

2. **Missing Setter Method**: Added `SetCorrelationId` method to make the interface complete and usable. Without a setter, implementations would have no standard way to establish the correlation ID. This is essential for the described functionality.

3. **Documentation Enhancement**: Updated the `<returns>` documentation to explicitly mention the null return possibility, improving API clarity.

4. **Interface Completeness**: The interface now follows the standard pattern for context-scoped services where both getter and setter are needed for proper lifecycle management.

5. **Consistency**: The code maintains all existing documentation quality and follows .NET 8 conventions for interface design and XML documentation.