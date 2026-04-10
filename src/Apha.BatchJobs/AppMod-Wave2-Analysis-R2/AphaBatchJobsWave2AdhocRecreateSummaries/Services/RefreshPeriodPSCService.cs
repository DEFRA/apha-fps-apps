using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to refresh Period_Proj_SubContract table for a given period.
    /// Converts usp_Refresh_Period_PSC stored procedure with delete-then-insert pattern.
    /// Implements joins between Proj_SubContract, tlkpProject, and CostCentre tables.
    /// Applies CASE logic for IsDefraProject field transformation.
    /// Enforces 300-second timeout and correlation-id logging.
    /// </summary>
    public interface IRefreshPeriodPSCService
    {
        /// <summary>
        /// Executes the Period_Proj_SubContract refresh operation for the specified period.
        /// </summary>
        /// <param name="period">The period number to refresh (1-12).</param>
        /// <param name="correlationId">Correlation identifier for logging and tracing.</param>
        /// <param name="cancellationToken">Cancellation token for operation timeout control.</param>
        /// <returns>True if operation succeeded, false otherwise.</returns>
        Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of Period_Proj_SubContract refresh service.
    /// Executes DELETE followed by INSERT with multi-table JOIN logic.
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
        /// Executes the refresh operation: DELETE existing period data, then INSERT fresh data.
        /// </summary>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "usp_Refresh_Period_PSC";
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
                    // Step 1: DELETE existing period data
                    await ExecuteDeleteAsync(connection, transaction, period, correlationId, cancellationToken);

                    // Step 2: INSERT fresh period data with joins and transformations
                    await ExecuteInsertAsync(connection, transaction, period, correlationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation(
                        "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms",
                        correlationId, stepName, DateTime.UtcNow, duration.TotalMilliseconds);

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
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms. Error: {ErrorMessage}",
                    correlationId, stepName, duration.TotalMilliseconds, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Executes DELETE operation to remove existing period data.
        /// SQL: DELETE FROM Period_Proj_SubContract WHERE period = @period
        /// </summary>
        private async Task ExecuteDeleteAsync(
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

            deleteCommand.Parameters.AddWithValue("@period", period);

            var rowsDeleted = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Deleted {RowCount} rows from Period_Proj_SubContract for period {Period}",
                correlationId, rowsDeleted, period);
        }

        /// <summary>
        /// Executes INSERT operation with multi-table JOIN and CASE transformation.
        /// Joins: Proj_SubContract INNER JOIN tlkpProject, CostCentre LEFT JOIN tlkpProject.
        /// Transforms IsDefraProject: 0 -> 'No', else -> 'Yes'.
        /// </summary>
        private async Task ExecuteInsertAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            string correlationId,
            CancellationToken cancellationToken)
        {
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
                    @period AS ""Period"",
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
                    ON psc.""Project"" = tp.""ParentProject""
                LEFT JOIN ""CostCentre"" cc 
                    ON tp.""CostCentre"" = cc.""CostCentre""";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction)
            {
                CommandTimeout = CommandTimeoutSeconds,
                CommandType = CommandType.Text
            };

            insertCommand.Parameters.AddWithValue("@period", period);

            var rowsInserted = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogDebug(
                "[{CorrelationId}] Inserted {RowCount} rows into Period_Proj_SubContract for period {Period}",
                correlationId, rowsInserted, period);
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for NpgsqlConnection and NpgsqlTransaction to ensure proper async disposal in .NET 8
// 2. Changed 'using' to 'await using' for NpgsqlCommand objects to ensure proper async disposal
// 3. Changed 'var stepName' to 'const string stepName' since it's a constant value
// 4. Fixed comment in ExecuteInsertAsync: Changed "RIGHT OUTER JOIN" to "LEFT JOIN" to match the actual SQL query
// 5. All other logic, error handling, transaction management, and logging remain unchanged as per requirements