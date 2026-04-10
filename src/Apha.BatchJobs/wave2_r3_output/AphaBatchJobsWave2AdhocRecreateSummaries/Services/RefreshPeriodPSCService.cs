using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to refresh Period_Proj_SubContract table by deleting existing period data 
    /// and inserting recalculated subcontract data via PostgreSQL.
    /// Implements DELETE then INSERT with joins across Proj_SubContract, tlkpProject, CostCentre tables.
    /// Converted from usp_Refresh_Period_PSC stored procedure.
    /// </summary>
    public interface IRefreshPeriodPSCService
    {
        /// <summary>
        /// Executes the refresh operation for the specified period.
        /// </summary>
        /// <param name="period">The period number (1-12) to refresh</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token with 300-second timeout</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of Period_Proj_SubContract refresh service.
    /// Executes DELETE followed by INSERT with multi-table joins to recalculate subcontract data.
    /// </summary>
    public class RefreshPeriodPSCService : IRefreshPeriodPSCService
    {
        private readonly string _connectionString;
        private readonly ILogger<RefreshPeriodPSCService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public RefreshPeriodPSCService(
            string connectionString,
            ILogger<RefreshPeriodPSCService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the refresh operation: DELETE existing period data, then INSERT recalculated data.
        /// </summary>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "RefreshPeriodPSC";
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} started at {StartTime:O} for period {Period}",
                    correlationId, stepName, startTime, period);

                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use IsolationLevel.ReadCommitted for better concurrency in PostgreSQL
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                try
                {
                    var deletedRows = await DeleteExistingPeriodDataAsync(connection, transaction, period, correlationId, cancellationToken);
                    var insertedRows = await InsertRecalculatedDataAsync(connection, transaction, period, correlationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed successfully in {Duration}ms for period {Period}. Deleted: {DeletedRows}, Inserted: {InsertedRows}",
                        correlationId, stepName, duration.TotalMilliseconds, period, deletedRows, insertedRows);

                    return true;
                }
                catch
                {
                    // Rollback only if transaction is still active
                    if (transaction.Connection != null)
                    {
                        await transaction.RollbackAsync(CancellationToken.None); // Use None to ensure rollback completes
                    }
                    throw;
                }
            }
            catch (OperationCanceledException ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled after {Duration}ms for period {Period}",
                    correlationId, stepName, duration.TotalMilliseconds, period);
                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms for period {Period}: {ErrorMessage}",
                    correlationId, stepName, duration.TotalMilliseconds, period, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deletes existing Period_Proj_SubContract records for the specified period.
        /// Equivalent to: DELETE FROM Period_Proj_SubContract WHERE period=@period
        /// </summary>
        /// <returns>Number of rows deleted</returns>
        private async Task<int> DeleteExistingPeriodDataAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string deleteSql = @"
                DELETE FROM ""Period_Proj_SubContract""
                WHERE ""Period"" = @period";

            await using var deleteCommand = new NpgsqlCommand(deleteSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            deleteCommand.Parameters.AddWithValue("@period", NpgsqlTypes.NpgsqlDbType.Integer, period);

            var rowsDeleted = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Deleted {RowCount} rows from Period_Proj_SubContract for period {Period}",
                correlationId, rowsDeleted, period);

            return rowsDeleted;
        }

        /// <summary>
        /// Inserts recalculated subcontract data for the specified period.
        /// Performs multi-table join across Proj_SubContract, tlkpProject, and CostCentre.
        /// Converts IsDefraProject boolean to 'Yes'/'No' string representation.
        /// </summary>
        /// <returns>Number of rows inserted</returns>
        private async Task<int> InsertRecalculatedDataAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            // Use parameterized query with explicit type casting for PostgreSQL
            const string insertSql = @"
                INSERT INTO ""Period_Proj_SubContract"" (
                    ""Period"",
                    ""SubContCounter"",
                    ""Project"",
                    ""OracleProjectCode"",
                    ""SubAccountCode"",
                    ""IsDefraProject"",
                    ""OPC"",
                    ""OCC"",
                    ""Month"",
                    ""Amount"",
                    ""AcctCode""
                )
                SELECT 
                    @period::integer,
                    psc.""SubContCounter"",
                    psc.""Project"",
                    tp.""OracleProjectCode"",
                    tp.""SubAccountCode"",
                    CASE 
                        WHEN tp.""IsDefraProject"" = 0 THEN 'No' 
                        ELSE 'Yes' 
                    END AS ""IsDefraProject"",
                    cc.""ProfitCentre"" AS ""OPC"",
                    cc.""CostCentre"" AS ""OCC"",
                    psc.""Month"",
                    psc.""Amount"",
                    psc.""AcctCode""
                FROM ""Proj_SubContract"" psc
                INNER JOIN ""tlkpProject"" tp 
                    ON tp.""ParentProject"" = psc.""Project""
                LEFT OUTER JOIN ""CostCentre"" cc 
                    ON cc.""CostCentre"" = tp.""CostCentre""";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            insertCommand.Parameters.AddWithValue("@period", NpgsqlTypes.NpgsqlDbType.Integer, period);

            var rowsInserted = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Inserted {RowCount} rows into Period_Proj_SubContract for period {Period}",
                correlationId, rowsInserted, period);

            return rowsInserted;
        }
    }
}


// Key improvements made:
// 1. Made stepName a const instead of var for better performance
// 2. Added explicit IsolationLevel.ReadCommitted for PostgreSQL transaction (default but explicit is better)
// 3. Changed return types of Delete/Insert methods to return row counts for better observability
// 4. Added row counts to success log message for better monitoring
// 5. Added null check before rollback to prevent exceptions if connection is already closed
// 6. Use CancellationToken.None for rollback to ensure it completes even if original token is cancelled
// 7. Changed OperationCanceledException logging from LogError to LogWarning (cancellation is expected behavior)
// 8. Added explicit NpgsqlDbType.Integer for parameters (better type safety with PostgreSQL)
// 9. Added ::integer cast in SQL for explicit type conversion (PostgreSQL best practice)
// 10. Used structured logging format {StartTime:O} for ISO 8601 timestamp format
// 11. Added exception parameter to LogWarning for OperationCanceledException for better diagnostics