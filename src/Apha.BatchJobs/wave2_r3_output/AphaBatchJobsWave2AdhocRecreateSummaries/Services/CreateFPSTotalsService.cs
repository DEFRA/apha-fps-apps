using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute creation/insertion of FPS totals data via PostgreSQL command execution.
    /// Converts legacy sp_createFPSTotals stored procedure logic to PostgreSQL-compatible implementation.
    /// Executes INSERT INTO FPSTotals with calculated aggregations using Npgsql with timeout handling and logging.
    /// </summary>
    public interface ICreateFPSTotalsService
    {
        /// <summary>
        /// Executes INSERT INTO FPSTotals with aggregated data from source tables using parameterized PostgreSQL command with 300-second timeout.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution across logs</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of FPS totals creation service.
    /// Converts legacy sp_createFPSTotals stored procedure to PostgreSQL INSERT statement.
    /// </summary>
    public class CreateFPSTotalsService : ICreateFPSTotalsService
    {
        private readonly string _connectionString;
        private readonly ILogger<CreateFPSTotalsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public CreateFPSTotalsService(
            string connectionString,
            ILogger<CreateFPSTotalsService> logger)
        {
            // Use ArgumentException.ThrowIfNullOrEmpty for .NET 8 (for string validation)
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            
            _connectionString = connectionString;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO FPSTotals with aggregated data from source tables.
        /// Implements legacy sp_createFPSTotals logic converted to PostgreSQL syntax.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token for timeout control</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "CreateFPSTotals";
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

                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // PostgreSQL uses lowercase identifiers by default; quoted identifiers are case-sensitive
                // Consider using lowercase unquoted identifiers for better PostgreSQL idiomatic code
                command.CommandText = @"
                    INSERT INTO ""FPSTotals"" 
                    SELECT DISTINCT 
                        ""qryFPSTotals"".""Project"",
                        ""qryFPSTotals"".""MonthNo"",
                        ""qryFPSTotals"".""Animals"",
                        ""qryFPSTotals"".""NonAnimals""
                    FROM ""qryFPSTotals""";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                var duration = DateTime.UtcNow - startTime;
                // Log as Warning instead of Error for cancellation scenarios (expected behavior)
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds);
                return false;
            }
            catch (PostgresException pgEx)
            {
                // Specific handling for PostgreSQL exceptions with error code
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error after {Duration}ms. SqlState: {SqlState}, Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds,
                    ex.Message);
                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async disposal**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` - proper async disposal pattern for .NET 8
2. **Connection string validation**: Enhanced validation to check for empty/whitespace strings, not just null
3. **Structured logging**: Added `:O` format specifier for DateTime to use ISO 8601 format in logs (better for log aggregation)
4. **Exception handling**: 
   - Changed `OperationCanceledException` logging from Error to Warning (cancellation is expected behavior, not an error)
   - Added specific `PostgresException` catch block to log PostgreSQL-specific error codes (SqlState) for better diagnostics
   - Included exception object in cancellation log for better traceability
5. **Const usage**: Changed `stepName` to `const` since it's a compile-time constant
6. **Code comments**: Added clarifying comment about PostgreSQL identifier casing conventions