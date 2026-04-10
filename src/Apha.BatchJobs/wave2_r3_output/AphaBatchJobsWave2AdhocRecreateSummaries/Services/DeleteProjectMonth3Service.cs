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
    /// Service to execute deletion of all records from ProjectMonth3 table.
    /// Implements PostgreSQL command execution with timeout handling and logging.
    /// Converts legacy sp_DeleteProjectMonth3 stored procedure logic.
    /// </summary>
    public class DeleteProjectMonth3Service
    {
        private readonly string _connectionString;
        private readonly ILogger<DeleteProjectMonth3Service> _logger;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteProjectMonth3Service.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <param name="logger">Logger instance for operation tracking</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString or logger is null</exception>
        public DeleteProjectMonth3Service(string connectionString, ILogger<DeleteProjectMonth3Service> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes DELETE FROM ProjectMonth3 using PostgreSQL command with 300-second timeout.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across services</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use Stopwatch for more accurate timing measurements instead of DateTime subtraction
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation(
                "[{CorrelationId}] DeleteProjectMonth3Service - Step started at {StartTime}",
                correlationId,
                DateTime.UtcNow);

            try
            {
                // Use await using for proper async disposal pattern in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for command disposal
                await using var command = new NpgsqlCommand("DELETE FROM \"ProjectMonth3\"", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Remove manual timeout CancellationTokenSource - CommandTimeout already handles this
                // Npgsql's CommandTimeout is sufficient and avoids redundant timeout mechanisms
                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();
                
                _logger.LogInformation(
                    "[{CorrelationId}] DeleteProjectMonth3Service - Step completed successfully. Rows affected: {RowsAffected}, Duration: {Duration}ms",
                    correlationId,
                    rowsAffected,
                    stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                // Handle explicit cancellation request separately for clarity
                stopwatch.Stop();
                
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] DeleteProjectMonth3Service - Operation was cancelled. Duration: {Duration}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (OperationCanceledException ex)
            {
                // Handle timeout scenario (CommandTimeout exceeded)
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] DeleteProjectMonth3Service - Step timed out after {Duration}ms (timeout: {Timeout}s)",
                    correlationId,
                    stopwatch.ElapsedMilliseconds,
                    CommandTimeoutSeconds);

                return false;
            }
            catch (PostgresException pgEx)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] DeleteProjectMonth3Service - PostgreSQL error occurred. Duration: {Duration}ms, SqlState: {SqlState}, Message: {Message}",
                    correlationId,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] DeleteProjectMonth3Service - Unexpected error occurred. Duration: {Duration}ms, Message: {Message}",
                    correlationId,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Stopwatch instead of DateTime**: Replaced `DateTime.UtcNow` subtraction with `Stopwatch` for more accurate and efficient timing measurements
2. **Async disposal pattern**: Changed `using` to `await using` for proper async disposal of `NpgsqlConnection` and `NpgsqlCommand` in .NET 8
3. **Removed redundant timeout mechanism**: Eliminated manual `CancellationTokenSource` for timeout since `CommandTimeout` property already handles this in Npgsql
4. **Improved cancellation handling**: Split `OperationCanceledException` into two catch blocks - one for explicit cancellation requests and one for timeouts
5. **Added XML documentation**: Added `<exception>` tag to constructor documentation
6. **Consistent stopwatch usage**: Ensured `stopwatch.Stop()` is called in all catch blocks before logging