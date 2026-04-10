using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_DeleteProjectMonth3 stored procedure.
    /// Deletes all records from ProjectMonth3 table.
    /// Implements exact SQL logic: DELETE FROM ProjectMonth3
    /// </summary>
    public class DeleteProjectMonth3Service
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonth3Service> _logger;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonth3Service(
            string connectionString,
            ILogger<DeleteProjectMonth3Service> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM projectmonth3 using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control</param>
        /// <returns>Tuple containing success status and optional error message</returns>
        public async Task<(bool Success, string? ErrorMessage)> ExecuteAsync(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
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
                // Use await using for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand("DELETE FROM projectmonth3", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

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

                return (true, null);
            }
            catch (OperationCanceledException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return (false, $"Step {stepName} was cancelled after {duration.TotalSeconds:F2} seconds");
            }
            catch (PostgresException pgEx)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {Duration}ms. SqlState: {SqlState}, Severity: {Severity}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Severity);

                return (false, $"PostgreSQL error in {stepName}: {pgEx.MessageText ?? pgEx.Message} (SqlState: {pgEx.SqlState})");
            }
            catch (NpgsqlException npgsqlEx)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    npgsqlEx,
                    "[{CorrelationId}] Step {StepName} failed with Npgsql error at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return (false, $"Database connection error in {stepName}: {npgsqlEx.Message}");
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return (false, $"Unexpected error in {stepName}: {ex.Message}");
            }
        }
    }
}


**Key improvements made:**

1. **Nullable reference types**: Changed return type to `string?` for ErrorMessage to be explicit about nullability (.NET 8 best practice)

2. **Input validation**: Added validation for correlationId parameter to prevent null/empty values

3. **Async disposal**: Changed `using` to `await using` for NpgsqlConnection and NpgsqlCommand to properly dispose async resources

4. **Const for stepName**: Made stepName a const instead of var since it never changes

5. **Structured logging**: Added `:O` format specifier for DateTime to use ISO 8601 format for better log parsing

6. **Exception handling improvements**:
   - Changed OperationCanceledException log level from Error to Warning (cancellation is expected behavior)
   - Added separate catch for NpgsqlException before generic Exception for better error categorization
   - Enhanced PostgresException logging with Severity property
   - Used MessageText property for PostgresException (more detailed than Message)
   - Improved error messages with more context

7. **Duration formatting**: Added `:F2` format specifier for duration seconds to show 2 decimal places

8. **Exception parameter**: Added exception parameter to OperationCanceledException catch for better logging context