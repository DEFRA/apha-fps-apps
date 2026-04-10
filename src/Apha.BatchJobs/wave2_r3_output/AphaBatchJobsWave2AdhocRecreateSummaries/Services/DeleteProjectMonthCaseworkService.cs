using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute deletion of all records from ProjectMonthCasework table.
    /// Implements DELETE FROM ProjectMonthCasework using PostgreSQL command execution
    /// with timeout handling and comprehensive logging.
    /// </summary>
    public class DeleteProjectMonthCaseworkService
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonthCaseworkService> _logger;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteProjectMonthCaseworkService class.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <param name="logger">Logger instance for diagnostic output</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null</exception>
        public DeleteProjectMonthCaseworkService(
            string connectionString,
            ILogger<DeleteProjectMonthCaseworkService> logger)
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(logger);
            
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonthCasework using PostgreSQL command with 300-second timeout.
        /// Returns success/failure status with detailed logging of operation metrics.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking the operation across logs</param>
        /// <param name="cancellationToken">Cancellation token to support operation cancellation</param>
        /// <returns>
        /// A tuple containing:
        /// - success: true if deletion completed successfully, false otherwise
        /// - rowsAffected: number of rows deleted from the table
        /// - errorMessage: error description if operation failed, null otherwise
        /// </returns>
        public async Task<(bool success, int rowsAffected, string? errorMessage)> ExecuteAsync(
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            const string stepName = "DeleteProjectMonthCasework";

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                startTime);

            // Use await using for proper async disposal pattern in .NET 8
            await using var connection = new NpgsqlConnection(_connectionString);
            
            try
            {
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand(
                    "DELETE FROM \"ProjectMonthCasework\"",
                    connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return (true, rowsAffected, null);
            }
            catch (OperationCanceledException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return (false, 0, $"Operation cancelled after {duration.TotalSeconds:F2} seconds");
            }
            catch (PostgresException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime}. Duration: {Duration}ms. SqlState: {SqlState}, Message: {Message}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.SqlState,
                    ex.MessageText);

                return (false, 0, $"PostgreSQL error: {ex.MessageText} (SqlState: {ex.SqlState})");
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

                return (false, 0, $"Unexpected error: {ex.Message}");
            }
            // Connection disposal is handled automatically by await using
        }
    }
}


**Key improvements made:**

1. **Modern .NET 8 null checking**: Replaced `?? throw new ArgumentNullException()` with `ArgumentNullException.ThrowIfNull()` for cleaner, more idiomatic .NET 8 code.

2. **Async disposal pattern**: Replaced manual disposal in finally block with `await using` statements, which is the recommended pattern in .NET 8 for async disposable resources.

3. **Nullable reference types**: Added `?` to `errorMessage` return type to properly indicate it can be null.

4. **Const for stepName**: Changed `stepName` from a variable to a const since it never changes.

5. **Removed redundant CloseAsync**: Connection disposal automatically closes the connection, so explicit `CloseAsync()` is unnecessary.

6. **Simplified resource management**: Eliminated manual null checks and disposal logic, reducing code complexity and potential for resource leaks.

7. **Better exception message**: Changed "timed out" to "cancelled" for `OperationCanceledException` as it's more accurate (cancellation can occur for reasons other than timeout).

8. **Formatting consistency**: Added formatting to duration output for better readability.