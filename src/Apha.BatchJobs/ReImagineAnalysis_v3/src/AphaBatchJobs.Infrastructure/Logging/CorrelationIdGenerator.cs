using System;

namespace AphaBatchJobs.Infrastructure.Logging
{
    /// <summary>
    /// Interface for correlation id generation service.
    /// Defines contract for generating unique identifiers for job execution tracking.
    /// </summary>
    public interface ICorrelationIdGenerator
    {
        /// <summary>
        /// Generates a new unique correlation id.
        /// </summary>
        /// <returns>A unique correlation identifier</returns>
        string Generate();

        /// <summary>
        /// Generates a new unique correlation id with a custom prefix.
        /// </summary>
        /// <param name="prefix">The prefix to prepend to the correlation id</param>
        /// <returns>A unique correlation identifier with the specified prefix</returns>
        string GenerateWithPrefix(string prefix);
    }

    /// <summary>
    /// Service class to generate unique correlation identifiers for job execution tracking.
    /// Provides GUID-based generation with optional prefix support for categorization.
    /// </summary>
    public sealed class CorrelationIdGenerator : ICorrelationIdGenerator
    {
        /// <summary>
        /// Creates a new unique correlation id using Guid.NewGuid() formatted as uppercase string without hyphens.
        /// </summary>
        /// <returns>A unique correlation identifier as uppercase string without hyphens</returns>
        public string Generate()
        {
            // Using "N" format specifier produces a 32-digit hexadecimal string without hyphens
            return Guid.NewGuid().ToString("N").ToUpperInvariant();
        }

        /// <summary>
        /// Creates a correlation id with a custom prefix for categorization.
        /// Format: PREFIX-GUID where GUID is uppercase without hyphens.
        /// </summary>
        /// <param name="prefix">The prefix to prepend to the correlation id</param>
        /// <returns>A unique correlation identifier with the specified prefix in format PREFIX-GUID</returns>
        /// <exception cref="ArgumentException">Thrown when prefix is null, empty or whitespace</exception>
        public string GenerateWithPrefix(string prefix)
        {
            // Consolidated validation: ArgumentException.ThrowIfNullOrWhiteSpace is available in .NET 8+
            // For earlier versions, this approach is more idiomatic than separate null and whitespace checks
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Prefix cannot be null, empty or whitespace.", nameof(prefix));
            }

            var guid = Guid.NewGuid().ToString("N").ToUpperInvariant();
            return $"{prefix}-{guid}";
        }
    }
}


**Key improvements made:**

1. **Interface before implementation**: Moved the interface definition before the class for better code organization and readability.

2. **Sealed class**: Added `sealed` modifier to `CorrelationIdGenerator` since it's not designed for inheritance and sealing can provide minor performance benefits.

3. **Consolidated validation**: Combined the null and whitespace checks into a single `string.IsNullOrWhiteSpace()` check, which is more idiomatic and efficient. This eliminates redundant validation since `IsNullOrWhiteSpace` already handles null cases.

4. **Simplified exception**: Changed from `ArgumentNullException` + `ArgumentException` to a single `ArgumentException` with a consolidated message, which is more appropriate when using `IsNullOrWhiteSpace`.

5. **Improved exception message**: Made the error message more concise and consistent with .NET conventions.

6. **Added inline comment**: Clarified the "N" format specifier usage for better code documentation.