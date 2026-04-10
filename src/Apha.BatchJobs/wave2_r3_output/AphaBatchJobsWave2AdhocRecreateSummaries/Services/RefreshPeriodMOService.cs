using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to refresh Period_MonthlyOutput table by deleting existing period data 
    /// and inserting recalculated monthly output data via PostgreSQL.
    /// Implements DELETE then INSERT with joins across tlkpProject, CostCentre, 
    /// MonthlyOutput, WorkGroup, tlkpTestReqmt tables.
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
        /// Executes DELETE FROM Period_MonthlyOutput WHERE period=@period, 
        /// then executes INSERT INTO Period_MonthlyOutput with recalculated data.
        /// </summary>
        /// <param name="period">The period number to refresh (1-12)</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(
            int period,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var stepName = $"RefreshPeriodMO_Period{period}";
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} started at {StartTime}",
                    correlationId,
                    stepName,
                    startTime);

                // Use await using for automatic disposal
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for automatic disposal and rollback if not committed
                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    await DeleteExistingPeriodDataAsync(
                        connection,
                        transaction,
                        period,
                        correlationId,
                        cancellationToken);

                    await InsertRecalculatedDataAsync(
                        connection,
                        transaction,
                        period,
                        correlationId,
                        cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed successfully in {Duration}ms",
                        correlationId,
                        stepName,
                        duration.TotalMilliseconds);

                    return true;
                }
                catch
                {
                    // Rollback is handled automatically by await using if not committed
                    // But explicit rollback is clearer for intent
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds);
                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms: {ErrorMessage}",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds,
                    ex.Message);
                return false;
            }
        }

        private async Task DeleteExistingPeriodDataAsync(
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

            // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
            // The cancellationToken parameter will handle external cancellation requests
            var rowsDeleted = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Deleted {RowCount} rows from Period_MonthlyOutput for period {Period}",
                correlationId,
                rowsDeleted,
                period);
        }

        private async Task InsertRecalculatedDataAsync(
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
                    CASE 
                        WHEN tp.""IsDefraProject"" = 0 THEN 'No' 
                        ELSE 'Yes' 
                    END AS ""IsDefraProject"",
                    cc.""ProfitCentre"" AS ""OPC"",
                    cc.""CostCentre"" AS ""OCC"",
                    mo.""Month"",
                    wg.""ProfitCentre"" AS ""SPC"",
                    wg.""WorkGroup"",
                    wg.""CostCentre"" AS ""SCC"",
                    mo.""TestCode"",
                    mo.""Volume"",
                    tr.""UnitPrice"" AS ""TestPrice"",
                    CAST(tr.""UnitPrice"" * mo.""Volume"" AS DECIMAL(19,4)) AS ""TotalCost""
                FROM ""tlkpProject"" tp
                LEFT JOIN ""CostCentre"" cc ON tp.""CostCentre"" = cc.""CostCentre""
                INNER JOIN ""MonthlyOutput"" mo ON tp.""ParentProject"" = mo.""Buyer""
                INNER JOIN ""WorkGroup"" wg ON mo.""WorkGroup"" = wg.""WorkGroup""
                INNER JOIN ""tlkpTestReqmt"" tr 
                    ON mo.""Buyer"" = tr.""projectBuyerCode"" 
                    AND mo.""TestCode"" = tr.""TestCode""";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds
            };

            insertCommand.Parameters.AddWithValue("@period", period);

            // Remove redundant timeout CancellationTokenSource since CommandTimeout already handles this
            // The cancellationToken parameter will handle external cancellation requests
            var rowsInserted = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Inserted {RowCount} rows into Period_MonthlyOutput for period {Period}",
                correlationId,
                rowsInserted,
                period);
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for NpgsqlConnection and NpgsqlTransaction for proper async disposal in .NET 8
// 2. Changed 'using' to 'await using' for NpgsqlCommand instances for proper async disposal
// 3. Removed redundant CancellationTokenSource creation in delete/insert methods - CommandTimeout already handles command-level timeouts
// 4. Changed LogError to LogWarning for OperationCanceledException as cancellation is not necessarily an error condition
// 5. Simplified cancellation handling by relying on the passed cancellationToken and CommandTimeout property
// 6. Transaction rollback is now handled automatically by await using, but kept explicit rollback for clarity
// 7. All changes maintain existing functionality while following .NET 8 async/await best practices