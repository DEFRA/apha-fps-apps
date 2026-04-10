using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Infrastructure.Logging
{
    /// <summary>
    /// Middleware class to inject and manage correlation id throughout job execution lifecycle.
    /// Uses AsyncLocal storage for thread-safe context management across async operations.
    /// Provides centralized correlation id management for distributed tracing and log correlation.
    /// </summary>
    public sealed class CorrelationIdMiddleware
    {
        // AsyncLocal provides thread-safe storage that flows with async/await execution context
        // This ensures correlation id is maintained across async boundaries and thread switches
        private static readonly AsyncLocal<string?> _correlationId = new AsyncLocal<string?>();

        private readonly ILogger<CorrelationIdMiddleware> _logger;
        // Field is declared but never used - consider removing if ICorrelationIdGenerator is not needed
        // If it will be used in the future, keep it; otherwise, remove to follow YAGNI principle
        private readonly ICorrelationIdGenerator _correlationIdGenerator;

        /// <summary>
        /// Initializes a new instance of the CorrelationIdMiddleware class.
        /// </summary>
        /// <param name="logger">Logger instance for logging correlation id operations</param>
        /// <param name="correlationIdGenerator">Generator service for creating new correlation ids</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or correlationIdGenerator is null</exception>
        public CorrelationIdMiddleware(
            ILogger<CorrelationIdMiddleware> logger,
            ICorrelationIdGenerator correlationIdGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationIdGenerator = correlationIdGenerator ?? throw new ArgumentNullException(nameof(correlationIdGenerator));
        }

        /// <summary>
        /// Sets the correlation id in AsyncLocal storage for the current execution context.
        /// The correlation id will flow through all async operations in the current context.
        /// </summary>
        /// <param name="correlationId">The correlation id to set for the current execution context</param>
        /// <exception cref="ArgumentException">Thrown when correlationId is null, empty or whitespace</exception>
        public void SetCorrelationId(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation id cannot be null, empty or whitespace.", nameof(correlationId));
            }

            _correlationId.Value = correlationId;

            _logger.LogDebug("Correlation id set: {CorrelationId}", correlationId);
        }

        /// <summary>
        /// Retrieves the current correlation id from AsyncLocal storage.
        /// Returns null if no correlation id has been set for the current execution context.
        /// </summary>
        /// <returns>The current correlation id, or null if not set</returns>
        public string? GetCorrelationId()
        {
            var correlationId = _correlationId.Value;

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogDebug("No correlation id found in current execution context");
                return null;
            }

            return correlationId;
        }

        /// <summary>
        /// Clears the correlation id from AsyncLocal storage.
        /// Should be called after job completion to clean up execution context.
        /// </summary>
        public void ClearCorrelationId()
        {
            var currentCorrelationId = _correlationId.Value;

            _correlationId.Value = null;

            if (!string.IsNullOrWhiteSpace(currentCorrelationId))
            {
                _logger.LogDebug("Correlation id cleared: {CorrelationId}", currentCorrelationId);
            }
            else
            {
                _logger.LogDebug("Correlation id cleared (no id was set)");
            }
        }
    }
}


// Key improvements made:
// 1. Added nullable reference type annotations (string? and AsyncLocal<string?>) for better null safety
// 2. Updated GetCorrelationId() return type to string? to explicitly indicate it can return null
// 3. Added comment about unused _correlationIdGenerator field - consider removing if not needed
// 4. Code follows .NET best practices with proper null checking and exception handling
// 5. AsyncLocal usage is appropriate for maintaining context across async operations
// 6. Logging statements use structured logging with proper placeholders