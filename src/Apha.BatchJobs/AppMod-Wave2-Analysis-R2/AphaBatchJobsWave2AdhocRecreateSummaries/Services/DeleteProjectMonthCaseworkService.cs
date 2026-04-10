using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute DELETE FROM ProjectMonthCasework operation.
    /// Converts sp_DeleteProjectMonthCasework stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class DeleteProjectMonthCaseworkService
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonthCaseworkService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonthCaseworkService(
            string connectionString,
            ILogger<DeleteProjectMonthCaseworkService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonthCasework using parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id. Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate correlationId parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            var stepName = "DeleteProjectMonthCasework";
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
                await using var command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = "DELETE FROM \"ProjectMonthCasework\"",
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
                // The CommandTimeout property provides database-level timeout, which is more appropriate
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
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
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                throw;
            }
            catch (PostgresException pgEx)
            {
                // Specific handling for PostgreSQL exceptions for better diagnostics
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms. PostgreSQL Error Code: {SqlState}. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);

                return false;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
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

1. **Async disposal**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` - this is the .NET 8 best practice for async resource disposal
2. **Input validation**: Added validation for `correlationId` parameter to prevent null/empty values
3. **Removed redundant timeout handling**: Eliminated the `CancellationTokenSource` with timeout since `CommandTimeout` property already provides database-level timeout management, avoiding double timeout logic
4. **PostgreSQL-specific exception handling**: Added `PostgresException` catch block before generic exception to capture PostgreSQL-specific error codes (`SqlState`) for better diagnostics
5. **Consistent datetime formatting**: Added `:O` format specifier for ISO 8601 datetime logging for better log parsing and consistency
6. **Simplified cancellation token usage**: Directly pass the original `cancellationToken` to `ExecuteNonQueryAsync` without creating linked token sources unnecessarily