using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute usp_LogRecreateSummaries stored procedure.
    /// Logs the recreate summaries execution with user ID, period, and timestamp.
    /// Implements exact SQL logic conversion from legacy SQL Server stored procedure.
    /// </summary>
    public class LogRecreateSummariesService
    {
        private readonly ILogger<LogRecreateSummariesService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public LogRecreateSummariesService(
            ILogger<LogRecreateSummariesService> logger,
            string connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            
            _connectionString = connectionString;
        }

        /// <summary>
        /// Executes the log recreate summaries operation.
        /// Calls sp_get_sp_no stored procedure to get user ID output parameter (@mno).
        /// Inserts record into recreatesummaries_log table with userid (@mno), period (@month parameter), 
        /// and current timestamp (CURRENT_TIMESTAMP).
        /// Uses Npgsql with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// </summary>
        /// <param name="month">The period/month number to log</param>
        /// <param name="correlationId">Correlation ID for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        public async Task ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync started for month {Month}",
                correlationId,
                month);

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Step 1: Call sp_get_sp_no to get user ID
                var userNumber = await GetUserNumberAsync(connection, correlationId, cancellationToken);

                _logger.LogInformation(
                    "[{CorrelationId}] Retrieved user number: {UserNumber}",
                    correlationId,
                    userNumber ?? "NULL");

                // Step 2: Insert into recreatesummaries_log table
                await InsertLogRecordAsync(connection, userNumber, month, correlationId, cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync completed successfully in {Duration}ms",
                    correlationId,
                    duration.TotalMilliseconds);
            }
            catch (OperationCanceledException)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync was cancelled after {Duration}ms",
                    correlationId,
                    duration.TotalMilliseconds);
                throw;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync failed after {Duration}ms for month {Month}",
                    correlationId,
                    duration.TotalMilliseconds,
                    month);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the user number by calling the sp_get_sp_no stored procedure.
        /// </summary>
        private async Task<string> GetUserNumberAsync(
            NpgsqlConnection connection, 
            string correlationId, 
            CancellationToken cancellationToken)
        {
            await using var command = new NpgsqlCommand("sp_get_sp_no", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = CommandTimeoutSeconds
            };

            var outputParam = new NpgsqlParameter("mno", NpgsqlDbType.Varchar, 20)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync(cancellationToken);

            return outputParam.Value as string;
        }

        /// <summary>
        /// Inserts a log record into the recreatesummaries_log table.
        /// </summary>
        private async Task InsertLogRecordAsync(
            NpgsqlConnection connection,
            string userNumber,
            int month,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string insertSql = @"
                INSERT INTO recreatesummaries_log (userid, period, datedone)
                VALUES (@userid, @period, CURRENT_TIMESTAMP)";

            await using var command = new NpgsqlCommand(insertSql, connection)
            {
                CommandTimeout = CommandTimeoutSeconds
            };

            command.Parameters.Add(new NpgsqlParameter("@userid", NpgsqlDbType.Varchar)
            {
                Value = (object)userNumber ?? DBNull.Value
            });
            command.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Integer)
            {
                Value = month
            });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Inserted {RowsAffected} row(s) into recreatesummaries_log for period {Period}",
                correlationId,
                rowsAffected,
                month);
        }
    }
}


// Key improvements made:
// 1. Enhanced connection string validation to check for null/empty/whitespace
// 2. Extracted GetUserNumberAsync method for better separation of concerns and testability
// 3. Extracted InsertLogRecordAsync method for better code organization
// 4. Added explicit NpgsqlDbType for parameters to avoid type inference issues
// 5. Added OperationCanceledException handling separately for better cancellation tracking
// 6. Improved logging to include month parameter in error logs for better debugging
// 7. Changed outputParam.Value?.ToString() to outputParam.Value as string for cleaner null handling
// 8. Made insertSql a const for better performance (no repeated string allocation)
// 9. Added explicit parameter type specification for better PostgreSQL type mapping
// 10. Improved null logging to show "NULL" instead of empty string for clarity
// 11. Used NpgsqlTypes namespace import for cleaner code
// 12. Removed unnecessary variable assignment in GetUserNumberAsync