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
    /// Service to refresh Period_TimeCostCalcs table for a given period.
    /// Converts usp_Refresh_Period_TCC stored procedure with delete-then-insert pattern.
    /// Implements complex joins, CASE logic for DefraProject, 300-second timeout and correlation-id logging.
    /// </summary>
    public class RefreshPeriodTCCService
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
        /// Deletes existing records for the period, then inserts fresh data from TimeCostCalcs with joins.
        /// </summary>
        /// <param name="period">The period number (1-12) to refresh</param>
        /// <param name="correlationId">Correlation ID for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken = default)
        {
            const string stepName = "RefreshPeriodTCC";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime:O} for period {Period}",
                correlationId, stepName, startTime, period);

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use IsolationLevel.ReadCommitted for better concurrency in PostgreSQL
                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                try
                {
                    await DeleteExistingRecordsAsync(connection, transaction, period, correlationId, cancellationToken);
                    await InsertRefreshedRecordsAsync(connection, transaction, period, correlationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms",
                        correlationId, stepName, DateTime.UtcNow, duration.TotalMilliseconds);

                    return true;
                }
                catch
                {
                    // Rollback is automatically handled by transaction disposal, but explicit rollback is clearer
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (OperationCanceledException ex)
            {
                // Separate handling for cancellation to avoid logging as error
                var duration = DateTime.UtcNow - startTime;
                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId, stepName, DateTime.UtcNow, duration.TotalMilliseconds);

                return false;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId, stepName, DateTime.UtcNow, duration.TotalMilliseconds, ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Deletes existing Period_TimeCostCalcs records for the specified period.
        /// </summary>
        private async Task DeleteExistingRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            const string deleteSql = @"
                DELETE FROM ""Period_TimeCostCalcs""
                WHERE ""Period"" = @period";

            await using var command = new NpgsqlCommand(deleteSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            // Use strongly-typed parameter for better performance and type safety
            command.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Integer) { Value = period });

            var rowsDeleted = await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Deleted {RowCount} existing records from Period_TimeCostCalcs for period {Period}",
                correlationId, rowsDeleted, period);
        }

        /// <summary>
        /// Inserts refreshed Period_TimeCostCalcs records by joining multiple tables.
        /// Implements the complex SELECT with CASE logic for DefraProject field.
        /// </summary>
        private async Task InsertRefreshedRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
            // Optimized query with explicit JOIN syntax for better PostgreSQL query planning
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
                    we.""SPNumber"",
                    tcc.""ChargeRate"",
                    tcc.""Pay"",
                    tcc.""Nonpay"",
                    tcc.""Overhead"",
                    tcc.""Time"",
                    tcc.""Cost"" AS ""TotalCost""
                FROM ""TimeCostCalcs"" tcc
                INNER JOIN ""tblWGEmployee"" we ON we.""PACTid"" = tcc.""StaffID""
                INNER JOIN ""tlkpProject"" tp ON tp.""ParentProject"" = tcc.""Project""
                INNER JOIN ""WorkGroup"" wg ON tcc.""WorkGroup"" = wg.""WorkGroup""
                LEFT JOIN ""CostCentre"" cc ON tp.""CostCentre"" = cc.""CostCentre""";

            await using var command = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            // Use strongly-typed parameter for better performance and type safety
            command.Parameters.Add(new NpgsqlParameter("@period", NpgsqlDbType.Integer) { Value = period });

            var rowsInserted = await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation(
                "[{CorrelationId}] Inserted {RowCount} records into Period_TimeCostCalcs for period {Period}",
                correlationId, rowsInserted, period);
        }
    }
}


**Key improvements made:**

1. **Strongly-typed parameters**: Changed from `AddWithValue` to explicit `NpgsqlParameter` with `NpgsqlDbType.Integer` for better type safety and performance in PostgreSQL.

2. **Explicit IsolationLevel**: Added `IsolationLevel.ReadCommitted` to the transaction for better concurrency control in PostgreSQL (default behavior made explicit).

3. **Optimized JOIN syntax**: Restructured the INSERT query to use more standard JOIN syntax, starting from the main table (`TimeCostCalcs`) and joining outward. This helps PostgreSQL's query planner optimize better.

4. **OperationCanceledException handling**: Added separate catch block for cancellation to log as warning instead of error, following .NET best practices.

5. **Consistent datetime formatting**: Added `:O` format specifier for ISO 8601 datetime logging for better log parsing.

6. **Made stepName const**: Changed `stepName` to const since it never changes, following C# best practices.

7. **Removed redundant parentheses**: Simplified the JOIN structure for better readability while maintaining the same logic.