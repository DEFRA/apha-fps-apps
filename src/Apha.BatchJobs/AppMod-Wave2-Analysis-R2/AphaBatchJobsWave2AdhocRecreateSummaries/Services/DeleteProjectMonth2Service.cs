using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute DELETE FROM ProjectMonth2 operation.
    /// Converts sp_deleteProjectMonth2 stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class DeleteProjectMonth2Service
    {
        private readonly ILogger<DeleteProjectMonth2Service> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonth2Service(
            ILogger<DeleteProjectMonth2Service> logger,
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
        /// Executes DELETE FROM ProjectMonth2 using parameterized PostgreSQL command with 300-second timeout.
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

            const string stepName = "DeleteProjectMonth2";
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
                await using var command = new NpgsqlCommand("DELETE FROM \"ProjectMonth2\"", connection)
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
            catch (PostgresException pgEx)
            {
                // Specific handling for PostgreSQL exceptions with additional context
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {DurationMs}ms. PostgreSQL Error Code: {SqlState}, Severity: {Severity}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Severity,
                    pgEx.Message);

                return false;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {DurationMs}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` - proper async disposal pattern for .NET 8
2. **Removed redundant timeout handling**: Eliminated the `CancellationTokenSource` with timeout since `CommandTimeout` property already handles database command timeout
3. **Input validation**: Added validation for `correlationId` parameter and improved `connectionString` validation
4. **PostgreSQL-specific exception handling**: Added `PostgresException` catch block to log PostgreSQL-specific error details (SqlState, Severity)
5. **Consistent datetime formatting**: Added `:O` format specifier for ISO 8601 datetime logging
6. **Const for stepName**: Changed to `const` since it's a compile-time constant
7. **Structured logging property names**: Changed `Duration` to `DurationMs` for clarity in log property naming
8. **Removed unnecessary linked cancellation token**: Simplified cancellation handling by relying on the passed `cancellationToken` and `CommandTimeout`