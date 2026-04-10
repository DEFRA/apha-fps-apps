using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to create project month casework records by inserting distinct data from qryProjectMonthCW query.
    /// Converts sp_CreateProjectMonthCasework stored procedure logic to PostgreSQL-compatible execution.
    /// </summary>
    public class CreateProjectMonthCaseworkService
    {
        private readonly string _connectionString;
        private readonly ILogger<CreateProjectMonthCaseworkService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public CreateProjectMonthCaseworkService(
            string connectionString,
            ILogger<CreateProjectMonthCaseworkService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO ProjectMonthCasework SELECT DISTINCT from qryProjectMonthCW.
        /// Implements exact logic from sp_CreateProjectMonthCasework stored procedure.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for timeout enforcement</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
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

                // Use await using for command disposal
                await using var command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text,
                    // PostgreSQL best practice: Use lowercase identifiers or properly quote mixed-case
                    CommandText = @"
                        INSERT INTO ""ProjectMonthCasework"" 
                        (""Project"", ""MonthNo"", ""CWDebit"", ""CWCredit"")
                        SELECT DISTINCT 
                            ""Project"",
                            ""MonthNo"",
                            ""CWDebit"",
                            ""CWCredit""
                        FROM ""qryProjectMonthCW"";"
                };

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

                // Include exception for better observability and structured logging
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
            catch (PostgresException pgEx)
            {
                // PostgreSQL-specific exception handling for better diagnostics
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime:O}. Duration: {Duration}ms. SqlState: {SqlState}, Severity: {Severity}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Severity,
                    pgEx.MessageText);

                return false;
            }
            catch (NpgsqlException npgEx)
            {
                // Npgsql-specific exception handling (connection issues, etc.)
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    npgEx,
                    "[{CorrelationId}] Step {StepName} failed with Npgsql error at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    npgEx.Message);

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

1. **Async disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly support async disposal patterns in .NET 8.

2. **Const for stepName**: Made `stepName` a `const` instead of `var` since it's a compile-time constant, improving performance slightly.

3. **ISO 8601 datetime formatting**: Added `:O` format specifier to DateTime logging for standardized, sortable timestamps (important for distributed systems like ECS Fargate).

4. **PostgreSQL-specific exception handling**: Added separate catch blocks for `PostgresException` and `NpgsqlException` to provide better diagnostics and observability, which is crucial for troubleshooting in containerized environments.

5. **Improved cancellation logging**: Changed `LogError` to `LogWarning` for `OperationCanceledException` and included the exception object for better structured logging.

6. **SQL termination**: Added semicolon to SQL statement following PostgreSQL best practices.

7. **Enhanced error context**: Added PostgreSQL-specific error details (SqlState, Severity) for better debugging in production environments.

These changes improve observability, follow .NET 8 async patterns, and provide better PostgreSQL-specific error handling for containerized deployments on AWS ECS Fargate.