using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute usp_Refresh_Period_MO stored procedure logic.
    /// Deletes and re-inserts Period_MonthlyOutput records for given period.
    /// Converts SQL Server stored procedure to PostgreSQL implementation.
    /// </summary>
    public class RefreshPeriodMOService
    {
        private readonly string _connectionString;
        private readonly ILogger<RefreshPeriodMOService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public RefreshPeriodMOService(
            string connectionString,
            ILogger<RefreshPeriodMOService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the refresh period monthly output operation.
        /// Deletes existing records for the period and inserts fresh data.
        /// </summary>
        /// <param name="period">The period number to refresh</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of rows inserted</returns>
        public async Task<int> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "RefreshPeriodMO";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime} for period {Period}",
                correlationId, stepName, startTime, period);

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Step 1: Delete existing records for the period
                    await using (var deleteCommand = new NpgsqlCommand(
                        "DELETE FROM period_monthlyoutput WHERE period = @period",
                        connection,
                        transaction)
                    {
                        CommandTimeout = CommandTimeoutSeconds
                    })
                    {
                        deleteCommand.Parameters.AddWithValue("period", period);

                        var deletedRows = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
                        _logger.LogDebug(
                            "[{CorrelationId}] Deleted {DeletedRows} rows from period_monthlyoutput for period {Period}",
                            correlationId, deletedRows, period);
                    }

                    // Step 2: Insert fresh data
                    const string insertSql = @"
                        INSERT INTO period_monthlyoutput (
                            period,
                            project,
                            oracleprojectcode,
                            subaccountcode,
                            isdefraproject,
                            opc,
                            occ,
                            month,
                            spc,
                            workgroup,
                            scc,
                            testcode,
                            volume,
                            testprice,
                            totalcost
                        )
                        SELECT 
                            @period,
                            tlkpproject.parentproject AS project,
                            tlkpproject.oracleprojectcode,
                            tlkpproject.subaccountcode,
                            CASE tlkpproject.isdefraproject 
                                WHEN 0 THEN 'No' 
                                ELSE 'Yes' 
                            END AS isdefraproject,
                            costcentre.profitcentre AS opc,
                            costcentre.costcentre AS occ,
                            monthlyoutput.month,
                            workgroup.profitcentre AS spc,
                            workgroup.workgroup,
                            workgroup.costcentre AS scc,
                            monthlyoutput.testcode,
                            monthlyoutput.volume,
                            tlkptestreqmt.unitprice AS testprice,
                            CAST(tlkptestreqmt.unitprice * monthlyoutput.volume AS DECIMAL(19,4)) AS totalcost
                        FROM (
                            (tlkpproject 
                            LEFT JOIN costcentre ON tlkpproject.costcentre = costcentre.costcentre)
                            INNER JOIN (
                                monthlyoutput 
                                INNER JOIN workgroup ON monthlyoutput.workgroup = workgroup.workgroup
                            ) ON tlkpproject.parentproject = monthlyoutput.buyer
                        )
                        INNER JOIN tlkptestreqmt 
                            ON monthlyoutput.buyer = tlkptestreqmt.projectbuyercode
                            AND monthlyoutput.testcode = tlkptestreqmt.testcode";

                    int insertedRows;
                    await using (var insertCommand = new NpgsqlCommand(insertSql, connection, transaction)
                    {
                        CommandTimeout = CommandTimeoutSeconds
                    })
                    {
                        insertCommand.Parameters.AddWithValue("period", period);
                        insertedRows = await insertCommand.ExecuteNonQueryAsync(cancellationToken);
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var endTime = DateTime.UtcNow;
                    var duration = endTime - startTime;

                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed at {EndTime}. Duration: {Duration}ms. Inserted {InsertedRows} rows for period {Period}",
                        correlationId, stepName, endTime, duration.TotalMilliseconds, insertedRows, period);

                    return insertedRows;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms for period {Period}",
                    correlationId, stepName, duration.TotalMilliseconds, period);

                throw;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime}. Duration: {Duration}ms. Period: {Period}",
                    correlationId, stepName, endTime, duration.TotalMilliseconds, period);

                throw;
            }
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for proper async disposal of NpgsqlConnection, NpgsqlTransaction, and NpgsqlCommand objects (.NET 8 best practice)
// 2. Removed '@' prefix from parameter names - PostgreSQL uses named parameters without '@' prefix (Npgsql best practice)
// 3. Made 'stepName' and 'insertSql' constants for better performance and immutability
// 4. Wrapped NpgsqlCommand objects in 'await using' blocks to ensure proper disposal and prevent connection leaks
// 5. Changed OperationCanceledException log level from LogError to LogWarning - cancellation is expected behavior, not an error
// 6. Improved message for OperationCanceledException from "timed out" to "was cancelled" for accuracy
// 7. Declared 'insertedRows' outside the using block to maintain scope for return statement
// 8. Maintained all existing functionality without adding new features