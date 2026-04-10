using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert missing projects for months 1-12 into ProjectMonth table.
    /// Converts sp_InsertMissingProjects stored procedure with WHILE loop logic.
    /// Implements 300-second timeout and correlation-id logging per step.
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
        /// Executes loop from month 1 to 12. For each month, performs INSERT INTO ProjectMonth (Project, MonthNo)
        /// SELECT DISTINCT ParentProject, @month FROM tlkpProject LEFT JOIN ProjectMonth 
        /// ON tlkpProject.ParentProject = ProjectMonth.Project AND @month = ProjectMonth.MonthNo 
        /// WHERE ProjectMonth.Project IS NULL ORDER BY ParentProject.
        /// Uses parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across logs</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>True if all months processed successfully, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "InsertMissingProjects";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O}",
                correlationId,
                stepName,
                startTime);

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Prepare SQL once outside the loop for better performance
                const string insertSql = @"
                    INSERT INTO ""ProjectMonth"" (""Project"", ""MonthNo"")
                    SELECT DISTINCT 
                        tp.""ParentProject"",
                        @month AS ""MonthNo""
                    FROM ""tlkpProject"" tp
                    LEFT JOIN ""ProjectMonth"" pm 
                        ON tp.""ParentProject"" = pm.""Project""
                        AND @month = pm.""MonthNo""
                    WHERE pm.""Project"" IS NULL
                    ORDER BY tp.""ParentProject""";

                for (int month = 1; month <= 12; month++)
                {
                    var monthStepName = $"{stepName}_Month{month}";
                    var monthStartTime = DateTime.UtcNow;

                    _logger.LogInformation(
                        "[{CorrelationId}] Processing month {Month} started at {StartTime:O}",
                        correlationId,
                        month,
                        monthStartTime);

                    await using var command = new NpgsqlCommand(insertSql, connection)
                    {
                        CommandTimeout = CommandTimeoutSeconds,
                        CommandType = CommandType.Text
                    };

                    command.Parameters.AddWithValue("month", month);

                    int rowsAffected;
                    try
                    {
                        // Use the provided cancellationToken directly; NpgsqlCommand respects CommandTimeout
                        rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        var monthDuration = DateTime.UtcNow - monthStartTime;
                        _logger.LogError(
                            "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms for month {Month}",
                            correlationId,
                            monthStepName,
                            monthDuration.TotalMilliseconds,
                            month);
                        return false;
                    }
                    catch (NpgsqlException ex) when (ex.InnerException is TimeoutException)
                    {
                        var monthDuration = DateTime.UtcNow - monthStartTime;
                        _logger.LogError(
                            ex,
                            "[{CorrelationId}] Step {StepName} timed out after {Duration}ms for month {Month}",
                            correlationId,
                            monthStepName,
                            monthDuration.TotalMilliseconds,
                            month);
                        return false;
                    }

                    var monthEndTime = DateTime.UtcNow;
                    var monthExecutionDuration = monthEndTime - monthStartTime;

                    _logger.LogInformation(
                        "[{CorrelationId}] Processing month {Month} completed at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                        correlationId,
                        month,
                        monthEndTime,
                        monthExecutionDuration.TotalMilliseconds,
                        rowsAffected);
                }

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return true;
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


// Key improvements made:
// 1. Removed redundant CancellationTokenSource creation - NpgsqlCommand already respects CommandTimeout property
// 2. Moved SQL query outside the loop as a const - no need to recreate the string 12 times
// 3. Changed parameter name from "@month" to "month" - PostgreSQL uses positional or named parameters without @ prefix
// 4. Added specific NpgsqlException handling for timeout scenarios to distinguish from cancellation
// 5. Made stepName a const since it never changes
// 6. Added ISO 8601 format (:O) to DateTime logging for better consistency and parseability
// 7. Simplified exception handling by removing nested using statements for CancellationTokenSource
// 8. Improved resource management - command is properly disposed per iteration with await using