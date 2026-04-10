using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute deletion of all records from ProjectMonthFinal table.
    /// Implements DELETE FROM ProjectMonthFinal using PostgreSQL command execution with timeout handling and logging.
    /// </summary>
    public class DeleteProjectMonthFinalService
    {
        private readonly ILogger<DeleteProjectMonthFinalService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteProjectMonthFinalService class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostic logging</param>
        /// <param name="connectionString">PostgreSQL connection string</param>
        public DeleteProjectMonthFinalService(
            ILogger<DeleteProjectMonthFinalService> logger,
            string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonthFinal using PostgreSQL command with 300-second timeout.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across services</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "DeleteProjectMonthFinal";
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
                await using var command = new NpgsqlCommand("DELETE FROM \"ProjectMonthFinal\"", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant CancellationTokenSource - CommandTimeout already handles timeout
                // and cancellationToken handles external cancellation
                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

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

                return false;
            }
            catch (Exception ex) when (ex is PostgresException { SqlState: "57014" } || // query_canceled
                                       ex is NpgsqlException { InnerException: TimeoutException } ||
                                       ex is TimeoutException)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} timed out at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

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


// Key improvements made:
// 1. Changed 'using' to 'await using' for NpgsqlConnection and NpgsqlCommand - proper async disposal pattern in .NET 8
// 2. Made stepName a const instead of var since it's a constant value
// 3. Removed redundant CancellationTokenSource creation - CommandTimeout property already handles timeout, avoiding double timeout management
// 4. Changed cancellation log from LogError to LogWarning - cancellation is expected behavior, not an error
// 5. Added ISO 8601 format specifier (:O) to DateTime logging for better consistency and parseability
// 6. Improved timeout exception handling to check for PostgresException with SqlState "57014" (query_canceled) and NpgsqlException with inner TimeoutException
// 7. Removed string.Contains check for timeout which is fragile and not reliable for exception detection