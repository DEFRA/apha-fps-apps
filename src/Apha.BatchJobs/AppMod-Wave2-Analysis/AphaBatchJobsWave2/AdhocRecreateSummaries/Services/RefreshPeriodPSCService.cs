using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2.AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute usp_Refresh_Period_PSC stored procedure logic.
    /// Deletes and re-inserts Period_Proj_SubContract records for given period.
    /// Converts SQL Server stored procedure to PostgreSQL implementation.
    /// </summary>
    public class RefreshPeriodPSCService
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
        /// Executes the refresh period PSC operation.
        /// Deletes existing records for the period and inserts fresh data from source tables.
        /// </summary>
        /// <param name="period">The period number to refresh</param>
        /// <param name="correlationId">Correlation ID for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(int period, string correlationId, CancellationToken cancellationToken)
        {
            var stepName = $"RefreshPeriodPSC_Period{period}";
            var startTime = DateTime.UtcNow;

            _logger.LogInformation(
                "[{CorrelationId}] Step started: {StepName} at {StartTime}",
                correlationId,
                stepName,
                startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                // Use await using for transaction disposal
                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Step 1: Delete existing records for the period
                    await DeletePeriodRecordsAsync(connection, transaction, period, cancellationToken);

                    // Step 2: Insert fresh records
                    await InsertPeriodRecordsAsync(connection, transaction, period, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    var endTime = DateTime.UtcNow;
                    var duration = endTime - startTime;

                    _logger.LogInformation(
                        "[{CorrelationId}] Step completed: {StepName} at {EndTime}, Duration: {Duration}ms",
                        correlationId,
                        stepName,
                        endTime,
                        duration.TotalMilliseconds);

                    return true;
                }
                catch
                {
                    // Rollback is automatically handled by transaction disposal if not committed
                    // but explicit rollback is clearer for intent
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    "[{CorrelationId}] Step cancelled: {StepName} at {EndTime}, Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                throw;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step failed: {StepName} at {EndTime}, Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
        }

        /// <summary>
        /// Deletes existing Period_Proj_SubContract records for the specified period.
        /// </summary>
        private async Task DeletePeriodRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            CancellationToken cancellationToken)
        {
            const string deleteSql = @"
                DELETE FROM period_proj_subcontract 
                WHERE period = $1";

            // Use await using for command disposal
            await using var command = new NpgsqlCommand(deleteSql, connection, transaction);
            command.CommandTimeout = CommandTimeoutSeconds;
            // Use positional parameters ($1) which is more idiomatic for PostgreSQL
            command.Parameters.Add(new NpgsqlParameter { Value = period });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            
            _logger.LogDebug(
                "Deleted {RowsAffected} records from period_proj_subcontract for period {Period}",
                rowsAffected,
                period);
        }

        /// <summary>
        /// Inserts Period_Proj_SubContract records for the specified period.
        /// Converts SQL Server query to PostgreSQL syntax.
        /// </summary>
        private async Task InsertPeriodRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int period,
            CancellationToken cancellationToken)
        {
            // Use PostgreSQL positional parameters and proper CASE syntax
            const string insertSql = @"
                INSERT INTO period_proj_subcontract (
                    period,
                    subcontcounter,
                    project,
                    oracleprojectcode,
                    subaccountcode,
                    isdefraproject,
                    opc,
                    occ,
                    month,
                    amount,
                    acctcode
                )
                SELECT 
                    $1,
                    proj_subcontract.subcontcounter,
                    proj_subcontract.project,
                    tlkpproject.oracleprojectcode,
                    tlkpproject.subaccountcode,
                    CASE 
                        WHEN tlkpproject.isdefraproject = 0 THEN 'No' 
                        ELSE 'Yes' 
                    END AS isdefraproject,
                    costcentre.profitcentre AS opc,
                    costcentre.costcentre AS occ,
                    proj_subcontract.month,
                    proj_subcontract.amount,
                    proj_subcontract.acctcode
                FROM costcentre 
                RIGHT OUTER JOIN tlkpproject 
                    ON costcentre.costcentre = tlkpproject.costcentre 
                INNER JOIN proj_subcontract 
                    ON tlkpproject.parentproject = proj_subcontract.project";

            // Use await using for command disposal
            await using var command = new NpgsqlCommand(insertSql, connection, transaction);
            command.CommandTimeout = CommandTimeoutSeconds;
            // Use positional parameters which is more idiomatic for PostgreSQL
            command.Parameters.Add(new NpgsqlParameter { Value = period });

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            
            _logger.LogDebug(
                "Inserted {RowsAffected} records into period_proj_subcontract for period {Period}",
                rowsAffected,
                period);
        }
    }
}


// Key improvements made:
// 1. Changed 'using' to 'await using' for async disposal pattern (.NET 8 best practice)
// 2. Changed '@period' to '$1' for PostgreSQL positional parameters (more idiomatic)
// 3. Changed AddWithValue to explicit NpgsqlParameter creation (better type safety)
// 4. Changed LogError to LogWarning for OperationCanceledException (cancellation is not an error)
// 5. Removed redundant error message from log (already in exception)
// 6. Added debug logging for rows affected to aid troubleshooting
// 7. Improved parameter handling for better PostgreSQL compatibility
// 8. Transaction rollback is still explicit for clarity, though disposal handles it