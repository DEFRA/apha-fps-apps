namespace AphaBatchJobsFoundationV3.Core.Interfaces
{
    /// <summary>
    /// Interface defining contract for correlation ID management service throughout batch job execution.
    /// Provides methods to generate, set, and retrieve correlation IDs for tracking and logging purposes.
    /// </summary>
    public interface ICorrelationService
    {
        /// <summary>
        /// Gets the current correlation ID from the execution context.
        /// </summary>
        /// <returns>The current correlation ID as a string, or null if not set.</returns>
        string? GetCorrelationId();

        /// <summary>
        /// Sets the correlation ID for the current execution context.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set for the current context.</param>
        void SetCorrelationId(string correlationId);

        /// <summary>
        /// Generates a new unique correlation ID and returns it.
        /// </summary>
        /// <returns>A newly generated unique correlation ID as a string.</returns>
        string GenerateCorrelationId();
    }
}


// Changes made:
// 1. Added nullable reference type annotation (string?) to GetCorrelationId() return type
//    - This explicitly indicates that the method can return null, improving null-safety
//    - Aligns with modern C# nullable reference types feature (C# 8.0+)
//    - Makes the contract clearer and helps prevent null reference exceptions
//    - The XML documentation already mentions "or null if not set", so the signature should reflect this