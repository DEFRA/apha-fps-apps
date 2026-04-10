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
    /// Service to execute sp_deleteFPSTotals stored procedure logic.
    /// Deletes all records from the fpstotals table.
    /// </summary>
    public class DeleteFPSTotalsService
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteFPSTotalsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteFPSTotalsService class.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null</exception>
        public DeleteFPSTotalsService(string connectionString, ILogger<DeleteFPSTotalsService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM fpstotals with timeout and logging.
        /// </summary>
        /// <param name="correlationId">Correlation ID for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use ArgumentException.ThrowIfNullOrWhiteSpace for .NET 8 (available in .NET 7+)
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            const string stepName = "DeleteFPSTotals";
            
            // Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Use await using for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use const for SQL query to improve readability and maintainability
                const string deleteSql = "DELETE FROM fpstotals";
                
                await using var command = new NpgsqlCommand(deleteSql, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
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
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime}. Duration: {Duration}ms. Error Code: {ErrorCode}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.MessageText);

                return false;
            }
            catch (OperationCanceledException ocEx)
            {
                stopwatch.Stop();

                // Log as warning with exception for better diagnostics
                _logger.LogWarning(
                    ocEx,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                // Re-throw OperationCanceledException to allow proper cancellation handling upstream
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime}. Duration: {Duration}ms. Exception Type: {ExceptionType}",
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


// Key improvements made:
// 1. Added ArgumentException.ThrowIfNullOrWhiteSpace for correlationId validation (.NET 8 best practice)
// 2. Replaced DateTime calculations with Stopwatch for more accurate timing measurements
// 3. Made stepName a const since it never changes
// 4. Extracted SQL query to a const for better maintainability
// 5. Added MessageText to PostgresException logging for better diagnostics
// 6. Changed OperationCanceledException to re-throw instead of returning false (proper cancellation pattern)
// 7. Added exception parameter to OperationCanceledException logging
// 8. Added ExceptionType to generic exception logging for better diagnostics
// 9. Changed "timed out" message to "was cancelled" for OperationCanceledException (more accurate)
// 10. Added XML documentation for exception throwing in constructor
// 11. Used ElapsedMilliseconds directly instead of TotalMilliseconds for cleaner code