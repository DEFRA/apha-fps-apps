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
    /// Service to execute DELETE operation on ProjectMonthFinal table.
    /// Deletes all records from ProjectMonthFinal table.
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
        /// Executes DELETE FROM projectmonthfinal using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking the operation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Best Practice: Validate input parameters
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "DeleteProjectMonthFinal";
            
            // Best Practice: Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Best Practice: Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Best Practice: Use await using for command disposal
                await using var command = new NpgsqlCommand("DELETE FROM projectmonthfinal", connection)
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
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {DurationMs}ms. Error Code: {ErrorCode}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState);

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
            catch (OperationCanceledException)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                // Best Practice: Re-throw OperationCanceledException to allow proper cancellation handling upstream
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
        }
    }
}


**Key Improvements Made:**

1. **Stopwatch for Timing**: Replaced `DateTime.UtcNow` subtraction with `Stopwatch` for more accurate performance measurements
2. **Await Using Pattern**: Changed `using` to `await using` for proper async disposal in .NET 8
3. **Input Validation**: Added validation for `correlationId` parameter
4. **Const for Step Name**: Made `stepName` a const since it never changes
5. **ISO 8601 DateTime Format**: Added `:O` format specifier for consistent datetime logging
6. **Removed Redundant Error Messages**: Removed `Message` property from log templates since the exception already contains this information
7. **Consistent Naming**: Used `DurationMs` consistently in all log statements
8. **Proper Resource Disposal**: Ensured both connection and command use async disposal patterns