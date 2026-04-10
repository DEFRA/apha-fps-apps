using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to refresh Period_TimeCostCalcs table by deleting existing period data 
    /// and inserting recalculated time cost data via PostgreSQL.
    /// Implements DELETE then INSERT with joins across tblWGEmployee, tlkpProject, 
    /// CostCentre, TimeCostCalcs, WorkGroup tables.
    /// Converted from legacy usp_Refresh_Period_TCC stored procedure.
    /// </summary>
    public interface IRefreshPeriodTCCService
    {
        /// <summary>
        /// Executes the refresh operation for the specified period.
        /// </summary>
        /// <param name="period">The period number (1-12) to refresh.</param>
        /// <param name="correlationId">Correlation ID for logging and tracing.</param>
        /// <param name="cancellationToken">Cancellation token with 300-second timeout.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of Period_TimeCostCalcs refresh service.
    /// Executes DELETE followed by INSERT with complex joins to recalculate time cost data.
    /// </summary>
    public class RefreshPeriodTCCService : IRefreshPeriodTCCService
    {
        private readonly string _connectionString;
        private readonly ILogger<RefreshPeriodTCCService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public RefreshPeriodTCCService(
            string connectionString,
            ILogger<RefreshPeriodTCCService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the refresh operation for Period_TimeCostCalcs table.
        /// First deletes existing period data, then inserts recalculated data.
        /// </summary>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken)
        {
            var stepName = $"RefreshPeriodTCC_Period{period}";
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} started at {StartTime}",
                    correlationId, stepName, startTime);

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use IsolationLevel.ReadCommitted for better concurrency in batch operations
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                try
                {
                    var rowsDeleted = await DeleteExistingPeriodDataAsync(connection, transaction, period, correlationId, cancellationToken);
                    var rowsInserted = await InsertRecalculatedDataAsync(connection, transaction, period, correlationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed successfully in {Duration}ms. Deleted: {RowsDeleted}, Inserted: {RowsInserted}",
                        correlationId, stepName, duration.TotalMilliseconds, rowsDeleted, rowsInserted);

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
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    "[{CorrelationId}] Step {StepName} timed out after {Duration}ms",
                    correlationId, stepName, duration.TotalMilliseconds);
                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms: {ErrorMessage}",
                    correlationId, stepName, duration.TotalMilliseconds, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes existing Period_TimeCostCalcs records for the specified period.
        /// Converted from: DELETE FROM Period_TimeCostCalcs WHERE period=@period
        /// </summary>
        private async Task<int> DeleteExistingPeriodDataAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string deleteSql = @"
                DELETE FROM ""Period_TimeCostCalcs""
                WHERE ""Period"" = @period";

            await using var cmd = new NpgsqlCommand(deleteSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            // Use strongly-typed parameter to avoid implicit conversions
            cmd.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Integer) { Value = period });

            var rowsDeleted = await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Deleted {RowCount} rows from Period_TimeCostCalcs for period {Period}",
                correlationId, rowsDeleted, period);

            return rowsDeleted;
        }

        /// <summary>
        /// Inserts recalculated time cost data into Period_TimeCostCalcs.
        /// Performs complex joins across tblWGEmployee, tlkpProject, CostCentre, TimeCostCalcs, and WorkGroup.
        /// Converted from legacy usp_Refresh_Period_TCC INSERT statement.
        /// </summary>
        private async Task<int> InsertRecalculatedDataAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            // Refactored SQL for better readability and PostgreSQL best practices
            // Using explicit JOIN syntax and proper formatting
            const string insertSql = @"
                INSERT INTO ""Period_TimeCostCalcs"" (
                    ""Period"",
                    ""Project"",
                    ""OracleProjectCode"",
                    ""SubAccountCode"",
                    ""Month"",
                    ""DefraProject"",
                    ""OCC"",
                    ""OPC"",
                    ""SPC"",
                    ""SCC"",
                    ""Name"",
                    ""GradeCode"",
                    ""SPNumber"",
                    ""ChargeRate"",
                    ""Pay"",
                    ""Nonpay"",
                    ""Overhead"",
                    ""Time"",
                    ""TotalCost""
                )
                SELECT 
                    @period AS ""Period"",
                    tp.""ParentProject"" AS ""Project"",
                    tp.""OracleProjectCode"",
                    tp.""SubAccountCode"",
                    tcc.""Month"",
                    CASE 
                        WHEN tp.""IsDefraProject"" = 0 THEN 'No' 
                        ELSE 'Yes' 
                    END AS ""DefraProject"",
                    cc.""CostCentre"" AS ""OCC"",
                    cc.""ProfitCentre"" AS ""OPC"",
                    wg.""ProfitCentre"" AS ""SPC"",
                    wg.""CostCentre"" AS ""SCC"",
                    tcc.""Name"",
                    tcc.""GradeCode"",
                    wge.""SPNumber"",
                    tcc.""ChargeRate"",
                    tcc.""Pay"",
                    tcc.""Nonpay"",
                    tcc.""Overhead"",
                    tcc.""Time"",
                    tcc.""Cost"" AS ""TotalCost""
                FROM ""TimeCostCalcs"" tcc
                INNER JOIN ""tblWGEmployee"" wge ON wge.""PACTid"" = tcc.""StaffID""
                INNER JOIN ""tlkpProject"" tp ON tp.""ParentProject"" = tcc.""Project""
                INNER JOIN ""WorkGroup"" wg ON tcc.""WorkGroup"" = wg.""WorkGroup""
                LEFT JOIN ""CostCentre"" cc ON tp.""CostCentre"" = cc.""CostCentre""";

            await using var cmd = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            // Use strongly-typed parameter to avoid implicit conversions
            cmd.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Integer) { Value = period });

            var rowsInserted = await cmd.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Inserted {RowCount} rows into Period_TimeCostCalcs for period {Period}",
                correlationId, rowsInserted, period);

            return rowsInserted;
        }
    }
}


// Key improvements made:
// 1. Added explicit IsolationLevel.ReadCommitted for better concurrency control in batch operations
// 2. Changed DeleteExistingPeriodDataAsync and InsertRecalculatedDataAsync to return int for better observability
// 3. Used strongly-typed NpgsqlParameter with NpgsqlDbType.Integer to avoid implicit type conversions and improve performance
// 4. Refactored INSERT SQL query to use more standard JOIN syntax (moving FROM TimeCostCalcs first, then explicit INNER/LEFT JOINs)
// 5. Enhanced logging to include both deleted and inserted row counts in the success message
// 6. Improved SQL readability while maintaining the same logical structure
// 7. Added proper using of NpgsqlTypes namespace for type-safe parameters
// 8. Maintained all existing functionality without adding new features