using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_CreateProjectMonthCasework stored procedure logic.
    /// Inserts distinct project month casework records from qryProjectMonthCW view.
    /// Converts SQL Server stored procedure to PostgreSQL implementation.
    /// </summary>
    public class CreateProjectMonthCaseworkService
    {
        private readonly string _connectionString;
        private readonly ILogger<CreateProjectMonthCaseworkService> _logger;
        private const int CommandTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the CreateProjectMonthCaseworkService class.
        /// </summary>
        /// <param name="connectionString">PostgreSQL connection string</param>
        /// <param name="logger">Logger instance for diagnostic logging</param>
        public CreateProjectMonthCaseworkService(
            string connectionString,
            ILogger<CreateProjectMonthCaseworkService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO projectmonthcasework (project, monthno, cwdebit, cwcredit) 
        /// selecting DISTINCT project, monthno, cwdebit, cwcredit FROM qryprojectmonthcw.
        /// Uses Npgsql with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across steps</param>
        /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
        /// <returns>Number of rows inserted</returns>
        /// <exception cref="OperationCanceledException">Thrown when operation is cancelled or times out</exception>
        /// <exception cref="NpgsqlException">Thrown when database operation fails</exception>
        public async Task<int> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "CreateProjectMonthCasework";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                startTime);

            // Declare timeoutCts outside try block to access in catch block
            CancellationTokenSource? timeoutCts = null;

            try
            {
                timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(CommandTimeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                int rowsAffected;

                await using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(linkedCts.Token).ConfigureAwait(false);

                    // PostgreSQL best practice: Use lowercase identifiers to avoid case sensitivity issues
                    const string sql = @"
                        INSERT INTO projectmonthcasework (project, monthno, cwdebit, cwcredit)
                        SELECT DISTINCT 
                            project,
                            monthno,
                            cwdebit,
                            cwcredit
                        FROM qryprojectmonthcw";

                    await using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.CommandTimeout = CommandTimeoutSeconds;
                        rowsAffected = await command.ExecuteNonQueryAsync(linkedCts.Token).ConfigureAwait(false);
                    }
                }

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed at {EndTime}. Duration: {Duration}ms. Rows inserted: {RowsAffected}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return rowsAffected;
            }
            catch (OperationCanceledException ex) when (timeoutCts?.Token.IsCancellationRequested ?? false)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} timed out after {Duration}ms",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds);

                throw new TimeoutException($"Step {stepName} exceeded timeout of {CommandTimeoutSeconds} seconds", ex);
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

                throw;
            }
            finally
            {
                // Properly dispose of timeoutCts to prevent resource leaks
                timeoutCts?.Dispose();
            }
        }
    }
}


**Key improvements made:**

1. **Resource Management**: Moved `timeoutCts` declaration outside the try block and added proper disposal in a `finally` block to prevent resource leaks.

2. **ConfigureAwait(false)**: Added `.ConfigureAwait(false)` to all async calls to avoid unnecessary context capturing, which is a best practice for library/service code and improves performance in ECS Fargate environments.

3. **Const for stepName**: Changed `stepName` from `var` to `const string` since it's a compile-time constant, improving performance slightly.

4. **Const for SQL**: Changed SQL string to `const` for better performance and clarity.

5. **Simplified using statements**: Removed unnecessary nested using blocks for `linkedCts` by using a single `using var` declaration.

6. **Comment added**: Added a comment about PostgreSQL identifier case sensitivity best practice.