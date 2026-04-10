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
    /// Service to execute DELETE FROM timecostcalcs operation.
    /// Converts sp_deleteTimeCostCalcs stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class DeleteTimeCostCalcsService
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteTimeCostCalcsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public DeleteTimeCostCalcsService(
            string connectionString,
            ILogger<DeleteTimeCostCalcsService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM timecostcalcs using parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id. Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use ArgumentException.ThrowIfNullOrWhiteSpace for .NET 8 (available in .NET 7+)
            ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

            const string stepName = "sp_deleteTimeCostCalcs";
            
            // Use Stopwatch for more accurate duration measurement
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Use await using for async disposal (C# 8.0+)
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = "DELETE FROM timecostcalcs",
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
                // The CommandTimeout property is the recommended way to handle timeouts in ADO.NET
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
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            catch (Exception ex) when (ex is PostgresException { SqlState: "57014" } || // query_canceled
                                       ex is NpgsqlException { InnerException: PostgresException { SqlState: "57014" } } ||
                                       ex is TimeoutException ||
                                       (ex is NpgsqlException npgEx && npgEx.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)))
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} timed out at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime}. Duration: {Duration}ms. Error: {ErrorMessage}",
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

1. **Added input validation**: Used `ArgumentException.ThrowIfNullOrWhiteSpace` (.NET 8 best practice) to validate `correlationId`
2. **Replaced DateTime calculations with Stopwatch**: More accurate for measuring elapsed time and avoids potential clock adjustment issues
3. **Changed to `await using`**: Modern C# async disposal pattern for better resource management
4. **Removed redundant timeout handling**: `CommandTimeout` property already handles database command timeouts; creating a separate `CancellationTokenSource` for timeout is redundant and can cause confusion
5. **Improved timeout exception detection**: Added PostgreSQL-specific error code `57014` (query_canceled) for more precise timeout detection
6. **Used `StringComparison.OrdinalIgnoreCase`**: More explicit and performant string comparison
7. **Made `stepName` a const**: Since it's a constant value
8. **Used `stopwatch.ElapsedMilliseconds`**: Direct property access instead of calculating from TimeSpan