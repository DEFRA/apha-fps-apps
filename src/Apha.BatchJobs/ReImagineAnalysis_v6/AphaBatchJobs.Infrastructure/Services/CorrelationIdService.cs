using AphaBatchJobs.Core.Interfaces;

namespace AphaBatchJobs.Infrastructure.Services
{
    /// <summary>
    /// Service for managing correlation IDs across async contexts.
    /// Uses AsyncLocal to maintain correlation ID throughout the async call chain.
    /// </summary>
    public sealed class CorrelationIdService : ICorrelationIdService
    {
        // AsyncLocal ensures correlation ID flows through async/await calls
        private static readonly AsyncLocal<string?> _correlationId = new();

        /// <summary>
        /// Retrieves the current correlation ID from the async context.
        /// </summary>
        /// <returns>The correlation ID or empty string if not set.</returns>
        public string GetCorrelationId()
        {
            // Use null-coalescing operator for cleaner null handling
            return _correlationId.Value ?? string.Empty;
        }

        /// <summary>
        /// Sets the correlation ID in the async context.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set.</param>
        /// <exception cref="ArgumentException">Thrown when correlationId is null or whitespace.</exception>
        public void SetCorrelationId(string correlationId)
        {
            // Validate input to prevent setting invalid correlation IDs
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or whitespace.", nameof(correlationId));
            }

            _correlationId.Value = correlationId;
        }

        /// <summary>
        /// Generates a new correlation ID using GUID.
        /// Uses "N" format for compact representation without hyphens.
        /// </summary>
        /// <returns>A new correlation ID string.</returns>
        public string GenerateCorrelationId()
        {
            // Use "N" format for more compact correlation ID (32 chars vs 36)
            // This reduces log size and improves readability
            return Guid.NewGuid().ToString("N");
        }
    }
}


**Key Improvements Made:**

1. **Sealed class**: Added `sealed` modifier since this class is not intended for inheritance, improving performance slightly
2. **Nullable reference types**: Changed `AsyncLocal<string>` to `AsyncLocal<string?>` for better null handling in .NET 10
3. **XML documentation**: Added comprehensive XML comments for better IntelliSense and documentation generation
4. **Input validation**: Added validation in `SetCorrelationId` to prevent invalid correlation IDs
5. **GUID format optimization**: Used `ToString("N")` for more compact correlation IDs (32 characters without hyphens vs 36 with hyphens)
6. **Target-typed new**: Used `new()` instead of `new AsyncLocal<string?>()` for cleaner syntax (.NET 10 feature)
7. **Exception handling**: Added proper exception with descriptive message for invalid input