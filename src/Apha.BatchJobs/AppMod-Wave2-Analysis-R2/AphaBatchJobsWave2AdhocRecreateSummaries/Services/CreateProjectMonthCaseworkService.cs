using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert data into ProjectMonthCasework from qryProjectMonthCW view.
    /// Converts sp_CreateProjectMonthCasework stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class CreateProjectMonthCaseworkService
    {
        private readonly ILogger<CreateProjectMonthCaseworkService> _logger;
        private readonly string _connectionString;
        private const int CommandTimeoutSeconds = 300;

        public CreateProjectMonthCaseworkService(
            ILogger<CreateProjectMonthCaseworkService> logger,
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
        /// Executes INSERT INTO ProjectMonthCasework (Project, MonthNo, CWDebit, CWCredit) 
        /// SELECT DISTINCT Project, MonthNo, CWDebit, CWCredit FROM qryProjectMonthCW 
        /// using parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id. Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate correlationId parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));
            }

            const string stepName = "CreateProjectMonthCasework";
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

                // SQL query with proper formatting and indentation
                const string insertSql = @"
                    INSERT INTO ""ProjectMonthCasework"" 
                    (""Project"", ""MonthNo"", ""CWDebit"", ""CWCredit"")
                    SELECT DISTINCT 
                        ""Project"",
                        ""MonthNo"",
                        ""CWDebit"",
                        ""CWCredit""
                    FROM ""qryProjectMonthCW""";

                await using var command = new NpgsqlCommand(insertSql, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };

                // Removed redundant CancellationTokenSource - CommandTimeout handles timeout,
                // and cancellationToken is passed directly for external cancellation
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

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
            catch (OperationCanceledException ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
            catch (PostgresException ex)
            {
                // Specific handling for PostgreSQL exceptions
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {Duration}ms. SqlState: {SqlState}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.SqlState,
                    ex.Message);

                return false;
            }
            catch (NpgsqlException ex)
            {
                // Specific handling for Npgsql connection/network exceptions
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with database connection error at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

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


**Key improvements made:**

1. **Input validation**: Added validation for `correlationId` parameter and improved connection string validation
2. **Async disposal**: Changed `using` to `await using` for proper async disposal pattern in .NET 8
3. **Removed redundant timeout handling**: Removed the manual `CancellationTokenSource` for timeout since `CommandTimeout` property already handles this at the database level
4. **Improved exception handling**: Added specific catch blocks for `PostgresException` and `NpgsqlException` before the general exception handler for better diagnostics
5. **Log level adjustment**: Changed `OperationCanceledException` from `LogError` to `LogWarning` as cancellation is often intentional
6. **DateTime formatting**: Added `:O` format specifier for ISO 8601 timestamp formatting in logs
7. **Const for SQL**: Made SQL string a const for better performance
8. **Removed unnecessary condition**: Simplified `OperationCanceledException` catch - no need to check `IsCancellationRequested` as it's implicit