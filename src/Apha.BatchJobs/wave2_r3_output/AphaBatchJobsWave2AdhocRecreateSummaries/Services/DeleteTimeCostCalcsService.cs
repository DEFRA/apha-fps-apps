using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute deletion of all records from TimeCostCalcs table.
    /// Implements sp_deleteTimeCostCalcs: DELETE FROM timecostcalcs
    /// </summary>
    public class DeleteTimeCostCalcsService
    {
        private readonly ILogger<DeleteTimeCostCalcsService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public DeleteTimeCostCalcsService(
            ILogger<DeleteTimeCostCalcsService> logger,
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
        /// Executes DELETE FROM timecostcalcs using PostgreSQL command with 300-second timeout.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use Stopwatch for more accurate duration measurement
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var stepName = "DeleteTimeCostCalcs";

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Create timeout cancellation token source
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(CommandTimeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                // Use await using for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(linkedCts.Token);

                // Use parameterized command structure even for simple DELETE
                await using var command = new NpgsqlCommand("DELETE FROM timecostcalcs", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                var rowsAffected = await command.ExecuteNonQueryAsync(linkedCts.Token);

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully. Rows affected: {RowsAffected}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    rowsAffected,
                    stopwatch.ElapsedMilliseconds);

                return true;
            }
            // Handle external cancellation first (more specific)
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled externally. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
            // Handle timeout cancellation
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogError(
                    "[{CorrelationId}] Step {StepName} timed out after {Timeout} seconds. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    CommandTimeoutSeconds,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
            // Handle PostgreSQL-specific exceptions
            catch (PostgresException ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error. SqlState: {SqlState}, Message: {Message}, Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    ex.SqlState,
                    ex.MessageText,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
            // Handle Npgsql-specific exceptions (connection issues, etc.)
            catch (NpgsqlException ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with Npgsql error. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
            // Handle all other exceptions
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
        }
    }
}


**Key Improvements Made:**

1. **Stopwatch for Duration Measurement**: Replaced `DateTime.UtcNow` subtraction with `Stopwatch` for more accurate elapsed time measurement, which is a .NET best practice.

2. **Enhanced Connection String Validation**: Added `string.IsNullOrWhiteSpace` check instead of just null check to prevent empty or whitespace-only connection strings.

3. **Improved Exception Handling**: Added `NpgsqlException` catch block between `PostgresException` and generic `Exception` to handle Npgsql-specific errors (like connection failures) separately from PostgreSQL database errors.

4. **Better Logging**: 
   - Changed external cancellation log level from `LogError` to `LogWarning` since external cancellation is often intentional
   - Added `MessageText` property to PostgresException logging for better error diagnostics

5. **Code Comments**: Added clarifying comments for timeout handling and exception hierarchy.

6. **Consistent Async Patterns**: Maintained proper `await using` pattern for all disposable resources, which is the .NET 8 best practice.