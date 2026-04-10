using AphaBatchJobsFoundationV3.Core.Interfaces;

namespace AphaBatchJobsFoundationV3.Infrastructure.Services
{
    /// <summary>
    /// Implementation of ICorrelationService that provides thread-safe correlation ID management
    /// using AsyncLocal storage for tracking execution context across async operations.
    /// Generates GUID-based correlation IDs for distributed tracing and logging.
    /// </summary>
    public class CorrelationService : ICorrelationService
    {
        /// <summary>
        /// AsyncLocal storage field for maintaining correlation ID across async execution contexts.
        /// Ensures thread-safe access to correlation ID in multi-threaded and async scenarios.
        /// </summary>
        private static readonly AsyncLocal<string?> _correlationId = new();

        /// <summary>
        /// Gets the current correlation ID from the AsyncLocal storage context.
        /// If no correlation ID exists, generates a new one automatically.
        /// </summary>
        /// <returns>The current correlation ID as a string, or a newly generated one if not previously set.</returns>
        public string GetCorrelationId()
        {
            // Return existing correlation ID or generate a new one if not set
            return string.IsNullOrWhiteSpace(_correlationId.Value) 
                ? GenerateCorrelationId() 
                : _correlationId.Value;
        }

        /// <summary>
        /// Sets the correlation ID in the AsyncLocal storage for the current execution context.
        /// This correlation ID will flow through all async operations in the current context.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set for the current execution context.</param>
        public void SetCorrelationId(string correlationId)
        {
            _correlationId.Value = correlationId;
        }

        /// <summary>
        /// Generates a new unique GUID-based correlation ID, stores it in the AsyncLocal context,
        /// and returns it for immediate use in logging and tracking operations.
        /// </summary>
        /// <returns>A newly generated unique correlation ID as a string in GUID format.</returns>
        public string GenerateCorrelationId()
        {
            var newCorrelationId = Guid.NewGuid().ToString();
            _correlationId.Value = newCorrelationId;
            return newCorrelationId;
        }
    }
}


// Key improvements made:
// 1. Changed return type of GetCorrelationId() from string? to string since it always returns a non-null value
// 2. Used target-typed new expression for AsyncLocal initialization (new() instead of new AsyncLocal<string?>())
// 3. Simplified GetCorrelationId() method using ternary operator for better readability
// 4. Removed unnecessary null-forgiving operator since the method guarantees a non-null return value
