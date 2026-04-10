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
    /// Service to insert missing project-month combinations for months 1-12 via PostgreSQL command execution.
    /// Implements WHILE loop logic from sp_InsertMissingProjects using C# iteration.
    /// Preserves exact SQL execution order and side effects from legacy stored procedure.
    /// </summary>
    public class InsertMissingProjectsService
    {
        private readonly string _connectionString;
        private readonly ILogger<InsertMissingProjectsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public InsertMissingProjectsService(
            string connectionString,
            ILogger<InsertMissingProjectsService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loops through months 1-12, for each month executes INSERT INTO ProjectMonth (Project, MonthNo)
        /// SELECT DISTINCT tlkpProject.ParentProject, @month FROM tlkpProject LEFT JOIN ProjectMonth
        /// ON tlkpProject.ParentProject = ProjectMonth.Project AND @month = ProjectMonth.MonthNo
        /// WHERE ProjectMonth.Project IS NULL ORDER BY ParentProject
        /// using parameterized PostgreSQL commands with 300-second timeout per iteration.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control</param>
        /// <returns>True if all months processed successfully, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "[{CorrelationId}] InsertMissingProjectsService.ExecuteAsync started",
                correlationId);

            // Use Stopwatch for more accurate timing measurements instead of DateTime.UtcNow subtraction
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Use await using for proper async disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // SQL query defined once outside the loop for better performance
                const string sql = @"
                    INSERT INTO ""ProjectMonth"" (""Project"", ""MonthNo"")
                    SELECT DISTINCT tp.""ParentProject"", @month AS ""MonthNo""
                    FROM ""tlkpProject"" tp
                    LEFT JOIN ""ProjectMonth"" pm 
                        ON tp.""ParentProject"" = pm.""Project""
                        AND @month = pm.""MonthNo""
                    WHERE pm.""Project"" IS NULL
                    ORDER BY tp.""ParentProject""";

                for (int month = 1; month <= 12; month++)
                {
                    // Check cancellation before processing each month
                    cancellationToken.ThrowIfCancellationRequested();

                    var monthStopwatch = Stopwatch.StartNew();

                    _logger.LogInformation(
                        "[{CorrelationId}] Processing month {Month} of 12",
                        correlationId,
                        month);

                    await using var command = new NpgsqlCommand(sql, connection)
                    {
                        CommandTimeout = CommandTimeoutSeconds,
                        CommandType = CommandType.Text
                    };

                    // Use NpgsqlDbType for better type safety and performance
                    command.Parameters.Add(new NpgsqlParameter("@month", NpgsqlTypes.NpgsqlDbType.Integer) { Value = month });

                    try
                    {
                        // ExecuteNonQueryAsync respects CommandTimeout, no need for additional CancellationTokenSource
                        // Pass the original cancellationToken for cooperative cancellation
                        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                        monthStopwatch.Stop();

                        _logger.LogInformation(
                            "[{CorrelationId}] Month {Month} completed: {RowsAffected} rows inserted in {Duration}ms",
                            correlationId,
                            month,
                            rowsAffected,
                            monthStopwatch.ElapsedMilliseconds);
                    }
                    catch (PostgresException pgEx)
                    {
                        // Catch PostgreSQL-specific exceptions for better error handling
                        _logger.LogError(
                            pgEx,
                            "[{CorrelationId}] PostgreSQL error inserting missing projects for month {Month}. SqlState: {SqlState}",
                            correlationId,
                            month,
                            pgEx.SqlState);
                        return false;
                    }
                    catch (NpgsqlException npgsqlEx)
                    {
                        // Catch Npgsql-specific exceptions (connection issues, timeout, etc.)
                        _logger.LogError(
                            npgsqlEx,
                            "[{CorrelationId}] Npgsql error inserting missing projects for month {Month}",
                            correlationId,
                            month);
                        return false;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning(
                            "[{CorrelationId}] InsertMissingProjectsService.ExecuteAsync cancelled at month {Month}",
                            correlationId,
                            month);
                        throw; // Re-throw to allow proper cancellation handling upstream
                    }
                }

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] InsertMissingProjectsService.ExecuteAsync completed successfully in {Duration}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                return true;
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation separately to avoid logging as error
                stopwatch.Stop();
                _logger.LogWarning(
                    "[{CorrelationId}] InsertMissingProjectsService.ExecuteAsync was cancelled after {Duration}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);
                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] InsertMissingProjectsService.ExecuteAsync failed after {Duration}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Stopwatch instead of DateTime.UtcNow**: More accurate for measuring elapsed time and avoids potential issues with clock adjustments
2. **SQL query defined once**: Moved constant SQL string outside the loop to avoid repeated string allocation
3. **Proper NpgsqlParameter usage**: Used NpgsqlDbType.Integer for type safety and better performance
4. **Removed redundant timeout handling**: ExecuteNonQueryAsync already respects CommandTimeout, removed the duplicate CancellationTokenSource logic
5. **Better exception handling**: Added specific catches for PostgresException and NpgsqlException before generic Exception
6. **Proper cancellation handling**: Use ThrowIfCancellationRequested() and re-throw OperationCanceledException to allow proper upstream handling
7. **Separate cancellation logging**: Distinguish between cancellation (warning) and errors for better observability
8. **Consistent async disposal**: Ensured all IAsyncDisposable resources use await using