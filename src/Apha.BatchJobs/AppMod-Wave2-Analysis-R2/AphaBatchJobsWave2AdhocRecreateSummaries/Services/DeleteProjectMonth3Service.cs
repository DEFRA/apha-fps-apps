using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute DELETE FROM ProjectMonth3 operation.
    /// Converts sp_DeleteProjectMonth3 stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class DeleteProjectMonth3Service
    {
        private readonly ILogger<DeleteProjectMonth3Service> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonth3Service(
            ILogger<DeleteProjectMonth3Service> logger,
            string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Validate connection string is not null or whitespace
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }
            
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonth3 using parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id. Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate correlationId parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));
            }

            const string stepName = "DeleteProjectMonth3";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand("DELETE FROM \"ProjectMonth3\"", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant timeout CTS - CommandTimeout already handles this
                // The cancellationToken parameter is sufficient for external cancellation
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {DurationMs}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                // Re-throw to allow caller to handle cancellation appropriately
                throw;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                // Return false instead of swallowing exception for batch job error handling
                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` - proper async disposal pattern in .NET 8
2. **Removed redundant timeout handling**: Removed `CancellationTokenSource` for timeout since `CommandTimeout` property already handles database command timeout
3. **Input validation**: Added validation for `correlationId` parameter to prevent null/empty values
4. **Enhanced connection string validation**: Changed from null check to `IsNullOrWhiteSpace` for better validation
5. **Consistent datetime formatting**: Added `:O` format specifier for ISO 8601 datetime logging
6. **Const for stepName**: Changed to `const` since it's a compile-time constant
7. **Removed redundant error message logging**: Removed `ex.Message` from log since the exception object already contains this information
8. **Improved log property naming**: Changed `Duration` to `DurationMs` for clarity in structured logging