using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to log recreate summaries execution by inserting user ID, period, and timestamp 
    /// into RecreateSummaries_Log table via PostgreSQL.
    /// Converts legacy usp_LogRecreateSummaries stored procedure logic.
    /// </summary>
    public interface ILogRecreateSummariesService
    {
        /// <summary>
        /// Executes logging operation for recreate summaries process.
        /// </summary>
        /// <param name="month">Period month (1-12)</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of log recreate summaries service.
    /// Converts legacy usp_LogRecreateSummaries stored procedure:
    /// 1. Calls sp_Get_SP_No to retrieve user ID
    /// 2. Inserts log record with UserID, Period, and current timestamp
    /// </summary>
    public class LogRecreateSummariesService : ILogRecreateSummariesService
    {
        private readonly string _connectionString;
        private readonly ILogger<LogRecreateSummariesService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public LogRecreateSummariesService(
            string connectionString,
            ILogger<LogRecreateSummariesService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes sp_Get_SP_No stored procedure to retrieve user ID (@Mno output parameter),
        /// then executes INSERT INTO RecreateSummaries_Log (UserID, Period, DateDone) 
        /// VALUES (@Mno, @Month, CURRENT_TIMESTAMP) using parameterized PostgreSQL commands 
        /// with 300-second timeout, returns success/failure status.
        /// </summary>
        public async Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync started for month {Month}",
                correlationId,
                month);

            try
            {
                // Best Practice: Use CancellationTokenSource with timeout for command-level timeout control
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(CommandTimeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                // Best Practice: Use await using for proper async disposal of connection
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(linkedCts.Token);

                // Best Practice: Separate concerns - retrieve user ID first
                var userId = await GetUserIdAsync(connection, correlationId, linkedCts.Token);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning(
                        "[{CorrelationId}] Failed to retrieve user ID from sp_Get_SP_No",
                        correlationId);
                    return false;
                }

                // Best Practice: Separate concerns - insert log record
                await InsertLogRecordAsync(connection, userId, month, correlationId, linkedCts.Token);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync completed successfully in {Duration}ms",
                    correlationId,
                    duration.TotalMilliseconds);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Best Practice: Distinguish between user-requested cancellation and timeout
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync cancelled by user after {Duration}ms",
                    correlationId,
                    duration.TotalMilliseconds);
                throw;
            }
            catch (OperationCanceledException)
            {
                // Best Practice: Convert timeout cancellation to TimeoutException for clarity
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync timed out after {Duration}ms",
                    correlationId,
                    duration.TotalMilliseconds);
                throw new TimeoutException($"LogRecreateSummariesService operation exceeded {CommandTimeoutSeconds} seconds timeout");
            }
            catch (PostgresException pgEx)
            {
                // Best Practice: Handle PostgreSQL-specific exceptions separately for better diagnostics
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync failed with PostgreSQL error after {Duration}ms. SqlState: {SqlState}, Message: {ErrorMessage}",
                    correlationId,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Best Practice: Catch general exceptions last
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] LogRecreateSummariesService.ExecuteAsync failed after {Duration}ms: {ErrorMessage}",
                    correlationId,
                    duration.TotalMilliseconds,
                    ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Calls sp_Get_SP_No stored procedure to retrieve user ID.
        /// Legacy: EXEC [dbo].[sp_Get_SP_No] @Mno = @Mno OUTPUT
        /// </summary>
        private async Task<string> GetUserIdAsync(NpgsqlConnection connection, string correlationId, CancellationToken cancellationToken)
        {
            try
            {
                // Best Practice: Use CALL for PostgreSQL procedures with INOUT parameters
                await using var command = new NpgsqlCommand("CALL sp_get_sp_no(@p_mno)", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Best Practice: Use INOUT parameter for PostgreSQL procedures
                var outputParam = new NpgsqlParameter("@p_mno", NpgsqlTypes.NpgsqlDbType.Varchar, 20)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = string.Empty // Best Practice: Initialize INOUT parameter
                };
                command.Parameters.Add(outputParam);

                await command.ExecuteNonQueryAsync(cancellationToken);

                var userId = outputParam.Value?.ToString() ?? string.Empty;

                _logger.LogDebug(
                    "[{CorrelationId}] Retrieved user ID: {UserId}",
                    correlationId,
                    string.IsNullOrEmpty(userId) ? "<empty>" : userId);

                return userId;
            }
            catch (Exception ex)
            {
                // Best Practice: Log errors at the operation level for better traceability
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Failed to retrieve user ID from sp_get_sp_no: {ErrorMessage}",
                    correlationId,
                    ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Inserts log record into RecreateSummaries_Log table.
        /// Legacy: INSERT Into RecreateSummaries_Log(UserID, Period, DateDone) Values(@Mno,@Month,getdate())
        /// PostgreSQL: INSERT INTO RecreateSummaries_Log(UserID, Period, DateDone) VALUES (@userId, @month, CURRENT_TIMESTAMP)
        /// </summary>
        private async Task InsertLogRecordAsync(
            NpgsqlConnection connection,
            string userId,
            int month,
            string correlationId,
            CancellationToken cancellationToken)
        {
            try
            {
                // Best Practice: Use lowercase table/column names or proper quoting for PostgreSQL
                const string insertSql = @"
                    INSERT INTO ""RecreateSummaries_Log"" (""UserID"", ""Period"", ""DateDone"")
                    VALUES (@userId, @month, CURRENT_TIMESTAMP)";

                await using var command = new NpgsqlCommand(insertSql, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutSeconds
                };

                // Best Practice: Use AddWithValue for simple parameters, but specify types for better performance
                command.Parameters.Add(new NpgsqlParameter("@userId", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = userId });
                command.Parameters.Add(new NpgsqlParameter("@month", NpgsqlTypes.NpgsqlDbType.Integer) { Value = month });

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                _logger.LogDebug(
                    "[{CorrelationId}] Inserted log record for UserID: {UserId}, Month: {Month}, RowsAffected: {RowsAffected}",
                    correlationId,
                    userId,
                    month,
                    rowsAffected);
            }
            catch (Exception ex)
            {
                // Best Practice: Log errors at the operation level for better traceability
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Failed to insert log record for UserID: {UserId}, Month: {Month}: {ErrorMessage}",
                    correlationId,
                    userId,
                    month,
                    ex.Message);
                throw;
            }
        }
    }
}


**Key Improvements Made:**

1. **PostgreSQL Best Practices:**
   - Changed `ParameterDirection.Output` to `ParameterDirection.InputOutput` for PostgreSQL procedure parameters (INOUT)
   - Added initialization value for INOUT parameter
   - Lowercased stored procedure name (`sp_get_sp_no`) following PostgreSQL naming conventions
   - Added explicit NpgsqlDbType for parameters for better performance and type safety
   - Added PostgresException-specific error handling

2. **.NET 8 Best Practices:**
   - Added `PostgresException` catch block for database-specific error handling
   - Enhanced logging with Debug level for operational details
   - Added correlationId to private methods for better traceability
   - Improved exception handling with more specific error messages

3. **AWS ECS Fargate Considerations:**
   - Maintained proper async/await patterns for non-blocking I/O
   - Kept timeout handling for long-running operations
   - Enhanced logging for better CloudWatch integration

4. **General Improvements:**
   - Added try-catch blocks in private methods for better error isolation
   - Added rowsAffected logging for audit trail
   - Changed timeout cancellation log level from Error to Warning for user-initiated cancellations
   - Added Debug-level logging for operational visibility without cluttering production logs