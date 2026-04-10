using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_deleteProjectMonth2 stored procedure.
    /// Deletes all records from ProjectMonth2 table.
    /// Implements exact SQL logic: DELETE FROM ProjectMonth2
    /// </summary>
    public class DeleteProjectMonth2Service
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonth2Service> _logger;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonth2Service(
            string connectionString,
            ILogger<DeleteProjectMonth2Service> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM projectmonth2 using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Best Practice: Validate input parameters
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "DeleteProjectMonth2";
            
            // Best Practice: Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Best Practice: Use await using for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Best Practice: Use const for SQL commands to prevent SQL injection and improve readability
                const string deleteCommand = "DELETE FROM projectmonth2";
                
                await using var command = new NpgsqlCommand(deleteCommand, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

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
            catch (PostgresException pgEx)
            {
                stopwatch.Stop();

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {DurationMs}ms. SqlState: {SqlState}, Severity: {Severity}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.Severity);

                return false;
            }
            catch (NpgsqlException npgEx)
            {
                stopwatch.Stop();

                _logger.LogError(
                    npgEx,
                    "[{CorrelationId}] Step {StepName} failed with Npgsql error at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (OperationCanceledException ocEx)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    ocEx,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                // Best Practice: Re-throw cancellation exceptions to allow proper cancellation handling upstream
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime:O}. Duration: {DurationMs}ms. Exception Type: {ExceptionType}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    ex.GetType().Name);

                return false;
            }
        }
    }
}


**Key Improvements Applied:**

1. **Stopwatch for Timing**: Replaced `DateTime.UtcNow` subtraction with `Stopwatch` for more accurate performance measurements
2. **Input Validation**: Added validation for `correlationId` parameter to fail fast on invalid input
3. **Const for SQL Command**: Extracted SQL command to a const variable for better maintainability
4. **ISO 8601 DateTime Format**: Added `:O` format specifier for consistent, sortable datetime logging
5. **Improved Error Logging**: Added `Severity` to PostgresException logging and `ExceptionType` to generic exception logging
6. **Exception Handling**: Added exception parameter to `OperationCanceledException` logging for consistency
7. **Code Consistency**: Used `const` for `stepName` since it never changes
8. **ElapsedMilliseconds**: Used `stopwatch.ElapsedMilliseconds` directly instead of `TotalMilliseconds` for cleaner code
9. **Removed Redundant Message**: Removed redundant `Message` property from error logs since the exception is already logged