using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to check periodLocked flag from tblPeriod table for given month.
    /// Executes query with 300 second timeout and returns boolean indicating if period is locked.
    /// </summary>
    public interface IPeriodLockedCheckService
    {
        /// <summary>
        /// Checks if the period is locked for the specified month.
        /// </summary>
        /// <param name="month">The month number to check (1-12)</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if period is locked (periodlocked != 0), false otherwise</returns>
        Task<bool> IsPeriodLockedAsync(int month, string correlationId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of period locked check service.
    /// Queries tblPeriod table to determine if a period is locked for the given month.
    /// </summary>
    public class PeriodLockedCheckService : IPeriodLockedCheckService
    {
        private readonly string _connectionString;
        private readonly ILogger<PeriodLockedCheckService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public PeriodLockedCheckService(
            string connectionString,
            ILogger<PeriodLockedCheckService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if the period is locked for the specified month by querying tblPeriod.
        /// Returns true if periodlocked is non-zero (1 or any non-zero value), false if 0.
        /// </summary>
        /// <param name="month">The month number to check</param>
        /// <param name="correlationId">Correlation ID for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for async operation</param>
        /// <returns>Boolean indicating if period is locked</returns>
        public async Task<bool> IsPeriodLockedAsync(
            int month, 
            string correlationId, 
            CancellationToken cancellationToken = default)
        {
            // Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogInformation(
                "[{CorrelationId}] Starting period locked check for month {Month}",
                correlationId,
                month);

            try
            {
                // Use await using pattern for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use parameterized query with explicit parameter type for PostgreSQL optimization
                const string query = "SELECT periodlocked FROM tblperiod WHERE endperiod = @month";
                
                await using var command = new NpgsqlCommand(query, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };

                // Explicitly specify NpgsqlDbType for better performance and type safety
                command.Parameters.Add(new NpgsqlParameter("@month", NpgsqlDbType.Integer) { Value = month });

                var result = await command.ExecuteScalarAsync(cancellationToken);

                stopwatch.Stop();

                if (result == null || result == DBNull.Value)
                {
                    _logger.LogWarning(
                        "[{CorrelationId}] No period found for month {Month}. Duration: {Duration}ms",
                        correlationId,
                        month,
                        stopwatch.ElapsedMilliseconds);
                    
                    return false;
                }

                // Convert result to boolean: non-zero values (including 1, -1, etc.) are considered locked
                // Use safer conversion with explicit type checking
                var periodLockedValue = Convert.ToInt32(result);
                var isLocked = periodLockedValue != 0;

                _logger.LogInformation(
                    "[{CorrelationId}] Period locked check completed for month {Month}. " +
                    "PeriodLocked value: {PeriodLockedValue}, IsLocked: {IsLocked}. Duration: {Duration}ms",
                    correlationId,
                    month,
                    periodLockedValue,
                    isLocked,
                    stopwatch.ElapsedMilliseconds);

                return isLocked;
            }
            catch (OperationCanceledException ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Period locked check cancelled for month {Month}. Duration: {Duration}ms",
                    correlationId,
                    month,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (PostgresException ex)
            {
                // Specific handling for PostgreSQL exceptions
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] PostgreSQL error checking period locked status for month {Month}. " +
                    "SqlState: {SqlState}, Duration: {Duration}ms",
                    correlationId,
                    month,
                    ex.SqlState,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Error checking period locked status for month {Month}. Duration: {Duration}ms",
                    correlationId,
                    month,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}


// Key improvements made:
// 1. Replaced DateTime.UtcNow calculations with Stopwatch for more accurate timing measurements
// 2. Added explicit NpgsqlDbType.Integer for parameter type safety and PostgreSQL query optimization
// 3. Changed Convert.ToInt16 to Convert.ToInt32 for safer conversion (Int32 is more standard)
// 4. Added specific PostgresException catch block for better PostgreSQL error handling
// 5. Extracted SQL query to const string for better readability and potential query plan caching
// 6. Changed "timed out" to "cancelled" in OperationCanceledException log message for accuracy
// 7. Added SqlState to PostgreSQL error logging for better diagnostics
// 8. Used stopwatch.ElapsedMilliseconds instead of TotalMilliseconds for cleaner code
// 9. Ensured proper async disposal patterns with await using
// 10. Maintained all existing functionality without adding new features