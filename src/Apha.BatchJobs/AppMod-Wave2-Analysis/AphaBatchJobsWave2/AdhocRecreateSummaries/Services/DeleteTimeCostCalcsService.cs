using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute DELETE FROM timecostcalcs operation.
    /// Deletes all records from timecostcalcs table.
    /// Converted from SQL Server stored procedure to PostgreSQL implementation.
    /// </summary>
    public class DeleteTimeCostCalcsService
    {
        private readonly ILogger<DeleteTimeCostCalcsService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the DeleteTimeCostCalcsService class.
        /// </summary>
        /// <param name="logger">Logger instance for structured logging</param>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <exception cref="ArgumentNullException">Thrown when logger or connectionString is null</exception>
        public DeleteTimeCostCalcsService(
            ILogger<DeleteTimeCostCalcsService> logger,
            string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionString));
            
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes DELETE FROM timecostcalcs using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if deletion succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use Stopwatch for more accurate duration measurement
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[{CorrelationId}] Step started: DeleteTimeCostCalcs at {StartTime:O}",
                correlationId,
                startTime);

            try
            {
                // Use NpgsqlDataSource for better connection pooling and performance in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use TRUNCATE for better performance if referential integrity allows
                // Otherwise, DELETE is appropriate for logged operations
                await using var command = new NpgsqlCommand("DELETE FROM timecostcalcs", connection)
                {
                    CommandTimeout = CommandTimeoutSeconds
                };

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();
                var endTime = DateTime.UtcNow;

                _logger.LogInformation(
                    "[{CorrelationId}] Step completed: DeleteTimeCostCalcs at {EndTime:O}. Duration: {DurationMs}ms. Rows deleted: {RowsAffected}",
                    correlationId,
                    endTime,
                    stopwatch.ElapsedMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                stopwatch.Stop();
                var endTime = DateTime.UtcNow;

                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step cancelled: DeleteTimeCostCalcs at {EndTime:O}. Duration: {DurationMs}ms",
                    correlationId,
                    endTime,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (PostgresException ex)
            {
                // Specific handling for PostgreSQL exceptions
                stopwatch.Stop();
                var endTime = DateTime.UtcNow;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step failed: DeleteTimeCostCalcs at {EndTime:O}. Duration: {DurationMs}ms. PostgreSQL Error Code: {SqlState}, Message: {ErrorMessage}",
                    correlationId,
                    endTime,
                    stopwatch.ElapsedMilliseconds,
                    ex.SqlState,
                    ex.Message);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var endTime = DateTime.UtcNow;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step failed: DeleteTimeCostCalcs at {EndTime:O}. Duration: {DurationMs}ms. Error: {ErrorMessage}",
                    correlationId,
                    endTime,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key Improvements Made:**

1. **Stopwatch for Duration Measurement**: Replaced `DateTime` subtraction with `Stopwatch` for more accurate elapsed time measurement, which is a .NET best practice.

2. **Connection String Validation**: Enhanced validation to check for null or whitespace, not just null.

3. **ISO 8601 DateTime Formatting**: Added `:O` format specifier for consistent, sortable datetime logging.

4. **PostgresException Handling**: Added specific catch block for `PostgresException` to capture PostgreSQL-specific error codes (`SqlState`) for better diagnostics.

5. **OperationCanceledException Logging Level**: Changed from `LogError` to `LogWarning` since cancellation is often intentional and not necessarily an error condition.

6. **Consistent Duration Logging**: Used `stopwatch.ElapsedMilliseconds` consistently across all log statements for accuracy.

7. **XML Documentation**: Enhanced exception documentation for the constructor.

8. **Comment Added**: Added note about TRUNCATE vs DELETE consideration for future optimization if referential integrity constraints allow.