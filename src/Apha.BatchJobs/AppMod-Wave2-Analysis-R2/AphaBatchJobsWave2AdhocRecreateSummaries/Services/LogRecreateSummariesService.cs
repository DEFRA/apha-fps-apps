using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to log recreate summaries execution into RecreateSummaries_Log table.
    /// Converts usp_LogRecreateSummaries stored procedure with sp_Get_SP_No call.
    /// Enforces 300-second timeout and correlation-id logging.
    /// </summary>
    public interface ILogRecreateSummariesService
    {
        /// <summary>
        /// Logs the recreate summaries execution for the specified month.
        /// </summary>
        /// <param name="month">The month (1-12) for which summaries were recreated.</param>
        /// <param name="correlationId">Correlation identifier for tracking.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if logging succeeded, false otherwise.</returns>
        Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Implementation of ILogRecreateSummariesService.
    /// Executes sp_Get_SP_No to retrieve user ID (Mno), then inserts log record into RecreateSummaries_Log.
    /// </summary>
    public sealed class LogRecreateSummariesService : ILogRecreateSummariesService
    {
        private readonly string _connectionString;
        private readonly ILogger<LogRecreateSummariesService> _logger;
        private const int TimeoutSeconds = 300;

        public LogRecreateSummariesService(
            string connectionString,
            ILogger<LogRecreateSummariesService> logger)
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            ArgumentNullException.ThrowIfNull(logger);

            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Executes the log recreate summaries operation.
        /// Retrieves user ID via sp_Get_SP_No, then inserts log entry.
        /// </summary>
        public async Task<bool> ExecuteAsync(int month, string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "LogRecreateSummaries";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                startTime);

            try
            {
                // Create linked cancellation token with timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cts.Token);

                // Retrieve user MNO
                var userMno = await GetUserMnoAsync(connection, cts.Token);

                // Insert log record
                await InsertLogRecordAsync(connection, userMno, month, cts.Token);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // External cancellation requested
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
            catch (OperationCanceledException)
            {
                // Timeout occurred
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    "[{CorrelationId}] Step {StepName} timed out after {Timeout} seconds at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    TimeoutSeconds,
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
                    "[{CorrelationId}] Step {StepName} failed at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
        }

        /// <summary>
        /// Retrieves the user M number by calling sp_get_sp_no stored procedure.
        /// </summary>
        private async Task<string> GetUserMnoAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            await using var command = new NpgsqlCommand("SELECT sp_get_sp_no()", connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = TimeoutSeconds
            };

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Inserts a log record into RecreateSummaries_Log table.
        /// Uses parameterized query with explicit NpgsqlDbType for better performance and type safety.
        /// </summary>
        private async Task InsertLogRecordAsync(
            NpgsqlConnection connection,
            string userMno,
            int month,
            CancellationToken cancellationToken)
        {
            const string insertSql = @"
                INSERT INTO recreatesummaries_log (userid, period, datedone)
                VALUES (@UserID, @Period, CURRENT_TIMESTAMP)";

            await using var command = new NpgsqlCommand(insertSql, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = TimeoutSeconds
            };

            // Use explicit NpgsqlDbType for better performance and type safety
            command.Parameters.Add(new NpgsqlParameter("@UserID", NpgsqlDbType.Varchar) { Value = userMno });
            command.Parameters.Add(new NpgsqlParameter("@Period", NpgsqlDbType.Integer) { Value = month });

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}


**Key Improvements Made:**

1. **Sealed Class**: Added `sealed` modifier to prevent inheritance, improving performance and clarity of intent.

2. **ArgumentNullException.ThrowIfNull**: Replaced manual null checks with .NET 8's `ArgumentNullException.ThrowIfNull()` for more idiomatic code.

3. **Const for stepName**: Changed `stepName` to `const` since it's a compile-time constant.

4. **Explicit NpgsqlDbType**: Added explicit `NpgsqlDbType` to parameters for better type safety, performance, and to avoid implicit type conversions in PostgreSQL.

5. **LogWarning for Cancellation**: Changed cancellation log from `LogError` to `LogWarning` since external cancellation is not necessarily an error condition.

6. **Removed redundant error message**: Removed `ex.Message` from the error log since the exception object already contains this information.

7. **Improved Comments**: Enhanced inline comments for better code documentation.

8. **Parameter Creation**: Used explicit `NpgsqlParameter` constructor with type specification instead of `AddWithValue` to avoid potential type inference issues with PostgreSQL.