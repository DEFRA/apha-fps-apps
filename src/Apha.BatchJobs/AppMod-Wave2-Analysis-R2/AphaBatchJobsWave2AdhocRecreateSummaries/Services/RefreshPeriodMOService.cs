using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to refresh Period_MonthlyOutput table for a given period.
    /// Converts usp_Refresh_Period_MO stored procedure with delete-then-insert pattern.
    /// Implements complex joins, CASE logic for IsDefraProject, 300-second timeout and correlation-id logging.
    /// </summary>
    public class RefreshPeriodMOService
    {
        private readonly string _connectionString;
        private readonly ILogger<RefreshPeriodMOService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public RefreshPeriodMOService(string connectionString, ILogger<RefreshPeriodMOService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the refresh operation for Period_MonthlyOutput table.
        /// Deletes existing records for the period, then inserts fresh data from joined tables.
        /// </summary>
        /// <param name="period">The period number (1-12) to refresh</param>
        /// <param name="correlationId">Correlation ID for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken = default)
        {
            var stepName = $"RefreshPeriodMO_Period{period}";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
                correlationId,
                stepName,
                startTime);

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    await DeleteExistingRecordsAsync(connection, transaction, period, correlationId, cancellationToken);
                    await InsertRefreshedDataAsync(connection, transaction, period, correlationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

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
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
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
        /// Deletes existing records from Period_MonthlyOutput for the specified period.
        /// </summary>
        private async Task DeleteExistingRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string deleteSql = @"
                DELETE FROM ""Period_MonthlyOutput""
                WHERE ""Period"" = @period";

            await using var deleteCommand = new NpgsqlCommand(deleteSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds
            };
            deleteCommand.Parameters.AddWithValue("@period", period);

            var rowsDeleted = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Deleted {RowCount} rows from Period_MonthlyOutput for period {Period}",
                correlationId,
                rowsDeleted,
                period);
        }

        /// <summary>
        /// Inserts refreshed data into Period_MonthlyOutput by joining multiple tables.
        /// Applies CASE logic for IsDefraProject and calculates TotalCost.
        /// </summary>
        private async Task InsertRefreshedDataAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string insertSql = @"
                INSERT INTO ""Period_MonthlyOutput"" (
                    ""Period"",
                    ""Project"",
                    ""OracleProjectCode"",
                    ""SubAccountCode"",
                    ""IsDefraProject"",
                    ""OPC"",
                    ""OCC"",
                    ""Month"",
                    ""SPC"",
                    ""WorkGroup"",
                    ""SCC"",
                    ""TestCode"",
                    ""Volume"",
                    ""TestPrice"",
                    ""TotalCost""
                )
                SELECT
                    @period AS ""Period"",
                    tp.""ParentProject"" AS ""Project"",
                    tp.""OracleProjectCode"",
                    tp.""SubAccountCode"",
                    CASE WHEN tp.""IsDefraProject"" = 0 THEN 'No' ELSE 'Yes' END AS ""IsDefraProject"",
                    cc.""ProfitCentre"" AS ""OPC"",
                    cc.""CostCentre"" AS ""OCC"",
                    mo.""Month"",
                    wg.""ProfitCentre"" AS ""SPC"",
                    wg.""WorkGroup"",
                    wg.""CostCentre"" AS ""SCC"",
                    mo.""TestCode"",
                    mo.""Volume"",
                    ttr.""UnitPrice"" AS ""TestPrice"",
                    CAST(ttr.""UnitPrice"" * mo.""Volume"" AS NUMERIC(18,2)) AS ""TotalCost""
                FROM ""tlkpProject"" tp
                LEFT JOIN ""CostCentre"" cc ON tp.""CostCentre"" = cc.""CostCentre""
                INNER JOIN ""MonthlyOutput"" mo ON tp.""ParentProject"" = mo.""Buyer""
                INNER JOIN ""WorkGroup"" wg ON mo.""WorkGroup"" = wg.""WorkGroup""
                INNER JOIN ""tlkpTestReqmt"" ttr ON mo.""Buyer"" = ttr.""projectBuyerCode""
                    AND mo.""TestCode"" = ttr.""TestCode""
                WHERE mo.""Month"" = @period";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds
            };
            insertCommand.Parameters.AddWithValue("@period", period);

            var rowsInserted = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Inserted {RowCount} rows into Period_MonthlyOutput for period {Period}",
                correlationId,
                rowsInserted,
                period);
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for NpgsqlConnection and NpgsqlTransaction to properly dispose async resources in .NET 8
// 2. Changed 'using' to 'await using' for NpgsqlCommand objects for consistent async disposal
// 3. Removed redundant CancellationTokenSource creation in delete/insert methods - the CommandTimeout property already handles timeouts
// 4. Used object initializer syntax for NpgsqlCommand to set CommandTimeout more idiomatically
// 5. Changed LogError to LogWarning for OperationCanceledException as cancellation is not necessarily an error condition
// 6. Removed redundant error message from LogError call (already included in exception)
// 7. Passed cancellationToken directly to ExecuteNonQueryAsync instead of creating linked token sources (CommandTimeout handles SQL timeout)
// 8. Maintained all existing functionality without adding new features