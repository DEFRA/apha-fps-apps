using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_DeleteProjectMonthCasework stored procedure.
    /// Deletes all records from ProjectMonthCasework table.
    /// Implements exact SQL logic: DELETE FROM ProjectMonthCasework
    /// </summary>
    public class DeleteProjectMonthCaseworkService
    {
        private readonly ILogger<DeleteProjectMonthCaseworkService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public DeleteProjectMonthCaseworkService(
            ILogger<DeleteProjectMonthCaseworkService> logger,
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
        /// Executes DELETE FROM projectmonthcasework using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control</param>
        /// <returns>Tuple containing success status and optional error message</returns>
        public async Task<(bool Success, string ErrorMessage)> ExecuteAsync(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            // Validate correlationId parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));
            }

            const string stepName = "DeleteProjectMonthCasework";
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
                    CommandText = "DELETE FROM projectmonthcasework",
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

                return (false, $"Operation was cancelled after {duration.TotalSeconds:F2} seconds");
            }
            catch (PostgresException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {Duration}ms. SqlState: {SqlState}, Severity: {Severity}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.SqlState,
                    ex.Severity);

                return (false, $"Database error (SqlState: {ex.SqlState}): {ex.MessageText ?? ex.Message}");
            }
            catch (NpgsqlException ex)
            {
                // Catch Npgsql-specific exceptions separately for better diagnostics
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with Npgsql error at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return (false, $"Database connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime:O}. Duration: {Duration}ms. Exception Type: {ExceptionType}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.GetType().Name);

                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly support async disposal patterns in .NET 8
2. **Input validation**: Added validation for `correlationId` parameter and improved `connectionString` validation to check for whitespace
3. **Consistent datetime formatting**: Added `:O` format specifier for ISO 8601 datetime logging for better consistency and parseability
4. **Exception handling refinement**: 
   - Changed `OperationCanceledException` log level from `LogError` to `LogWarning` (cancellation is expected behavior, not an error)
   - Added separate catch for `NpgsqlException` to distinguish connection-level errors from PostgreSQL errors
   - Enhanced PostgreSQL exception logging with `Severity` and `MessageText` properties
   - Added exception type name to unexpected error logs for better diagnostics
5. **Numeric formatting**: Added format specifier `{duration.TotalSeconds:F2}` for consistent decimal formatting
6. **Const usage**: Changed `stepName` to `const` since it's a compile-time constant
7. **Error message improvements**: Enhanced error messages with more specific information (SqlState, formatted duration)