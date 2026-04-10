using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute sp_createFPSTotals stored procedure logic.
    /// Creates/inserts records into fpstotals table from qryFPSTotals query.
    /// </summary>
    public class CreateFPSTotalsService
    {
        private readonly string _connectionString;
        private readonly ILogger<CreateFPSTotalsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public CreateFPSTotalsService(
            string connectionString,
            ILogger<CreateFPSTotalsService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO fpstotals using Npgsql command with 300 second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Best Practice: Validate input parameters
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "sp_CreateFPSTotals";
            
            // Best Practice: Use Stopwatch for more accurate timing measurements
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Best Practice: Use ConfigureAwait(false) for library/service code to avoid deadlocks
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                // Best Practice: Use const for SQL queries to improve readability and maintainability
                const string insertSql = @"
                    INSERT INTO fpstotals
                    SELECT DISTINCT 
                        qryFPSTotals.project,
                        qryFPSTotals.monthno,
                        qryFPSTotals.fpscost,
                        qryFPSTotals.fpsinvoices,
                        qryFPSTotals.fpscoiw,
                        qryFPSTotals.fpsportsales
                    FROM qryFPSTotals";

                await using var command = new NpgsqlCommand(insertSql, connection)
                {
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (PostgresException pgEx)
            {
                stopwatch.Stop();

                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error at {EndTime}. Duration: {Duration}ms. Error Code: {ErrorCode}, Message: {ErrorMessage}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);

                return false;
            }
            // Best Practice: NpgsqlException can wrap timeout exceptions, catch it before generic TimeoutException
            catch (NpgsqlException npgsqlEx) when (npgsqlEx.InnerException is TimeoutException)
            {
                stopwatch.Stop();

                _logger.LogError(
                    npgsqlEx,
                    "[{CorrelationId}] Step {StepName} timed out at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (TimeoutException timeoutEx)
            {
                stopwatch.Stop();

                _logger.LogError(
                    timeoutEx,
                    "[{CorrelationId}] Step {StepName} timed out at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (OperationCanceledException cancelEx) when (cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();

                // Best Practice: Use LogWarning for expected cancellations, not errors
                _logger.LogWarning(
                    cancelEx,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with unexpected error at {EndTime}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key Improvements Made:**

1. **Stopwatch for Timing**: Replaced `DateTime.UtcNow` subtraction with `Stopwatch` for more accurate performance measurements
2. **ConfigureAwait(false)**: Added to all async calls to prevent potential deadlocks in library code
3. **Input Validation**: Added validation for `correlationId` parameter
4. **Const for SQL**: Changed `insertSql` to const for better performance and clarity
5. **NpgsqlException Handling**: Added specific catch for `NpgsqlException` wrapping timeout exceptions (common pattern in Npgsql)
6. **Cancellation Token Check**: Added condition `when (cancellationToken.IsCancellationRequested)` to properly handle cancellation
7. **Const for stepName**: Changed to const since it never changes
8. **ElapsedMilliseconds**: Using `stopwatch.ElapsedMilliseconds` directly instead of `TotalMilliseconds` for consistency