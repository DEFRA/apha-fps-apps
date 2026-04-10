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
    /// Service to execute sp_CreateTimeCostCalcs stored procedure logic.
    /// Inserts calculated time cost records into TimeCostCalcs table with complex joins and calculations.
    /// Converts SQL Server stored procedure to PostgreSQL with exact business logic preservation.
    /// </summary>
    public class CreateTimeCostCalcsService
    {
        private readonly string _connectionString;
        private readonly ILogger<CreateTimeCostCalcsService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public CreateTimeCostCalcsService(
            string connectionString,
            ILogger<CreateTimeCostCalcsService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO timecostcalcs with SELECT from multiple joined tables.
        /// Calculates chargerate based on isdefraproject, class based on sector_name,
        /// cost, pay, nonpay, and overhead values.
        /// Uses 300 second timeout and logs execution metrics with correlation id.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of rows inserted</returns>
        public async Task<int> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "CreateTimeCostCalcs";
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                DateTime.UtcNow);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // SQL query converted from SQL Server to PostgreSQL syntax
                // Preserves exact business logic from sp_CreateTimeCostCalcs
                // Note: PostgreSQL table/column names are case-sensitive; ensure proper casing
                command.CommandText = @"
                    INSERT INTO timecostcalcs (
                        workgroup,
                        jobcode,
                        project,
                        month,
                        staffid,
                        gradecode,
                        name,
                        chargerate,
                        class,
                        time,
                        cost,
                        division,
                        pay,
                        nonpay,
                        overhead
                    )
                    SELECT DISTINCT 
                        wgg.workgroup,
                        mt.timecode AS jobcode,
                        tcv.parentproject AS project,
                        mt.month,
                        vps.pactid AS staffid,
                        wgg.gradecode,
                        vps.name,
                        CASE 
                            WHEN tp.isdefraproject = 0 THEN pcg.chargerate
                            ELSE pcg.defrachargerate
                        END AS chargerate,
                        CASE 
                            WHEN tprog.sector_name = 'Charge' THEN 'Charge'
                            ELSE 'Free'
                        END AS class,
                        mt.hours AS time,
                        CASE 
                            WHEN tprog.sector_name = 'Charge' THEN 
                                mt.hours * CASE 
                                    WHEN tp.isdefraproject = 0 THEN pcg.chargerate
                                    ELSE pcg.defrachargerate
                                END
                            ELSE 0
                        END AS cost,
                        tpc.division,
                        mt.hours * pcg.payrate AS pay,
                        mt.hours * pcg.npr AS nonpay,
                        mt.hours * pcg.ohr AS overhead
                    FROM workgroupgrade wgg
                    INNER JOIN monthlytime mt ON wgg.workgroup = mt.workgroup AND wgg.gradecode = mt.gradecode
                    INNER JOIN timecodevalid tcv ON mt.timecode = tcv.timecode
                    INNER JOIN vpacttblstaff vps ON mt.staffid = vps.pactid
                    INNER JOIN profitcentregrade pcg ON wgg.profitcentre = pcg.profitcentre AND wgg.gradecode = pcg.gradecode
                    INNER JOIN tlkpproject tp ON tcv.parentproject = tp.parentproject
                    INNER JOIN tlkpprogram tprog ON tp.program = tprog.program
                    INNER JOIN tblkpprofitcentre tpc ON wgg.profitcentre = tpc.profitcentre";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                stopwatch.Stop();

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed at {EndTime}. Duration: {Duration}ms. Rows inserted: {RowsAffected}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    stopwatch.ElapsedMilliseconds,
                    rowsAffected);

                return rowsAffected;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
            catch (PostgresException pgEx)
            {
                // PostgreSQL-specific exception handling for better diagnostics
                stopwatch.Stop();
                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed with PostgreSQL error after {Duration}ms. SqlState: {SqlState}, Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds,
                    pgEx.SqlState,
                    pgEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }
}


**Key Improvements Made:**

1. **Async Disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly support async disposal patterns in .NET 8.

2. **Input Validation**: Added validation for `correlationId` parameter to prevent null/empty values from being processed.

3. **Const for Step Name**: Changed `stepName` from `var` to `const` since it's a compile-time constant that never changes.

4. **PostgreSQL-Specific Exception Handling**: Added explicit `PostgresException` catch block before the generic exception handler to capture PostgreSQL-specific errors (SqlState) for better diagnostics and troubleshooting.

5. **Improved Log Message**: Changed "timed out" to "was cancelled" for `OperationCanceledException` as this exception can be thrown for various cancellation reasons, not just timeouts.

6. **Code Comment**: Added note about PostgreSQL case-sensitivity for table/column names to remind developers of potential issues during deployment.