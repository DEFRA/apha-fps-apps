using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute DELETE FROM ProjectMonthFinal operation.
    /// Converts sp_DeleteProjectMonthFinal stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class DeleteProjectMonthFinalService
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonthFinalService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonthFinalService(
            string connectionString,
            ILogger<DeleteProjectMonthFinalService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonthFinal using parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id. Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "DeleteProjectMonthFinal";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = "DELETE FROM \"ProjectMonthFinal\"",
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
                // The CommandTimeout property provides database-level timeout, which is more appropriate
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {DurationMs}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            catch (PostgresException pgEx)
            {
                // Specific handling for PostgreSQL exceptions to capture database-specific errors
                stopwatch.Stop();

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {DurationMs}ms. PostgreSQL Error Code: {SqlState}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {DurationMs}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Replaced `using` with `await using`**: For proper async disposal of `NpgsqlConnection` and `NpgsqlCommand` in .NET 8
2. **Used `Stopwatch` instead of `DateTime` calculations**: More accurate and efficient for measuring elapsed time
3. **Removed redundant timeout `CancellationTokenSource`**: The `CommandTimeout` property already handles database command timeout at the driver level, making the additional CancellationTokenSource unnecessary and potentially confusing
4. **Added input validation**: Validates `correlationId` parameter to prevent null/empty values
5. **Added `PostgresException` catch block**: Specific handling for PostgreSQL exceptions to capture database-specific error codes (SqlState)
6. **Used ISO 8601 format for timestamps**: Added `:O` format specifier for consistent, sortable timestamp logging
7. **Made `stepName` a const**: Since it never changes within the method
8. **Improved log property naming**: Changed `Duration` to `DurationMs` for clarity