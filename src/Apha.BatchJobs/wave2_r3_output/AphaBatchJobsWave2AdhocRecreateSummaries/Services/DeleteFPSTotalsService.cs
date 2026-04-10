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
    /// Service to execute deletion of FPS totals data via PostgreSQL command execution.
    /// Executes DELETE FROM FPSTotals using Npgsql with timeout handling and logging.
    /// Implements the legacy sp_deleteFPSTotals stored procedure logic.
    /// </summary>
    public class DeleteFPSTotalsService
    {
        private readonly ILogger<DeleteFPSTotalsService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteFPSTotalsService class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostic logging</param>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or connectionString is null</exception>
        public DeleteFPSTotalsService(
            ILogger<DeleteFPSTotalsService> logger,
            string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Validate connection string is not null or whitespace
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionString));
            }
            
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes DELETE FROM FPSTotals using parameterized PostgreSQL command with 300-second timeout.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across services</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation(
                "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync started",
                correlationId);

            try
            {
                // Use await using for proper async disposal (C# 8.0+)
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand("DELETE FROM \"FPSTotals\"", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
                // The CommandTimeout property provides database-level timeout handling
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();
                
                _logger.LogInformation(
                    "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync completed successfully. " +
                    "Rows affected: {RowsAffected}, Duration: {DurationMs}ms",
                    correlationId,
                    rowsAffected,
                    stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                stopwatch.Stop();
                
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync was cancelled after {DurationMs}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                // Re-throw cancellation exceptions to allow proper cancellation handling upstream
                throw;
            }
            catch (PostgresException ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync failed with PostgreSQL error. " +
                    "SqlState: {SqlState}, Message: {Message}, Duration: {DurationMs}ms",
                    correlationId,
                    ex.SqlState,
                    ex.MessageText,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (NpgsqlException ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync failed with Npgsql error. " +
                    "Duration: {DurationMs}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] DeleteFPSTotalsService.ExecuteAsync failed with unexpected error. " +
                    "Duration: {DurationMs}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Stopwatch instead of DateTime**: Used `Stopwatch` for more accurate performance measurements, avoiding potential clock adjustments
2. **await using**: Changed to `await using` for proper async disposal of `NpgsqlConnection` and `NpgsqlCommand` (.NET 8 best practice)
3. **Removed redundant timeout handling**: Removed the manual `CancellationTokenSource` for timeout since `CommandTimeout` property already provides database-level timeout handling
4. **Enhanced connection string validation**: Added whitespace check for connection string validation
5. **Improved exception handling**: 
   - Changed `OperationCanceledException` log level to `LogWarning` and re-throw to allow proper cancellation propagation
   - Added specific `NpgsqlException` catch block before generic `Exception`
   - Added `MessageText` to PostgresException logging for better diagnostics
6. **Consistent timing**: Used `stopwatch.ElapsedMilliseconds` consistently throughout for better readability
7. **XML documentation**: Added exception documentation to constructor