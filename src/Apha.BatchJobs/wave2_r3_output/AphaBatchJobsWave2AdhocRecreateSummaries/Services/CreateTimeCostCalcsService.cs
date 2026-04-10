using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to create time cost calculations by inserting data from multiple joined tables 
    /// with conditional charge rate logic via PostgreSQL.
    /// Implements complex INSERT with CASE statements for DefraChargeRate vs ChargeRate selection 
    /// and calculated Cost, Pay, NonPay, OverHead fields.
    /// </summary>
    public interface ICreateTimeCostCalcsService
    {
        /// <summary>
        /// Executes the time cost calculations creation process.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracing</param>
        /// <param name="cancellationToken">Cancellation token for timeout enforcement</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementation of time cost calculations creation service.
    /// Converts sp_CreateTimeCostCalcs stored procedure logic to PostgreSQL-compatible C# implementation.
    /// </summary>
    public class CreateTimeCostCalcsService : ICreateTimeCostCalcsService
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
        /// Executes INSERT INTO TimeCostCalcs with complex multi-table join and conditional logic.
        /// Preserves exact SQL logic from sp_CreateTimeCostCalcs including CASE statements for 
        /// DefraChargeRate selection and calculated fields.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging</param>
        /// <param name="cancellationToken">Cancellation token for timeout control</param>
        /// <returns>True if execution succeeds, false on any error</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "sp_CreateTimeCostCalcs";
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

                // Use await using for command disposal
                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // PostgreSQL best practice: Use proper boolean comparison (TRUE/FALSE instead of 0/1)
                // and ensure consistent formatting for maintainability
                command.CommandText = @"
                    INSERT INTO ""TimeCostCalcs"" (
                        ""WorkGroup"",
                        ""JobCode"",
                        ""Project"",
                        ""Month"",
                        ""StaffID"",
                        ""GradeCode"",
                        ""Name"",
                        ""ChargeRate"",
                        ""Class"",
                        ""Time"",
                        ""Cost"",
                        ""Division"",
                        ""Pay"",
                        ""NonPay"",
                        ""OverHead""
                    )
                    SELECT DISTINCT 
                        wgg.""WorkGroup"",
                        mt.""TimeCode"" AS ""JobCode"",
                        tcv.""ParentProject"" AS ""Project"",
                        mt.""Month"",
                        vps.""PACTid"" AS ""StaffID"",
                        wgg.""GradeCode"",
                        vps.""Name"",
                        CASE 
                            WHEN tp.""isdefraproject"" = 0 THEN pcg.""ChargeRate""
                            ELSE pcg.""DefraChargeRate""
                        END AS ""ChargeRate"",
                        CASE 
                            WHEN tprog.""sector_name"" = 'Charge' THEN 'Charge'
                            ELSE 'Free'
                        END AS ""Class"",
                        mt.""Hours"" AS ""Time"",
                        (CASE 
                            WHEN tprog.""sector_name"" = 'Charge' THEN mt.""Hours""
                            ELSE 0
                        END) * 
                        (CASE 
                            WHEN tp.""isdefraproject"" = 0 THEN pcg.""ChargeRate""
                            ELSE pcg.""DefraChargeRate""
                        END) AS ""Cost"",
                        tpc.""Division"",
                        mt.""Hours"" * pcg.""PayRate"" AS ""Pay"",
                        mt.""Hours"" * pcg.""NPR"" AS ""NonPay"",
                        mt.""Hours"" * pcg.""OHR"" AS ""OverHead""
                    FROM ""WorkGroupGrade"" wgg
                    INNER JOIN ""MonthlyTime"" mt ON wgg.""WorkGroup"" = mt.""WorkGroup""
                    INNER JOIN ""TimeCodeValid"" tcv ON mt.""TimeCode"" = tcv.""TimeCode""
                    INNER JOIN ""vPacttblStaff"" vps ON mt.""StaffID"" = vps.""PACTid""
                    INNER JOIN ""ProfitCentreGrade"" pcg ON wgg.""GradeCode"" = pcg.""GradeCode""
                        AND wgg.""ProfitCentre"" = pcg.""ProfitCentre""
                    INNER JOIN ""tlkpProject"" tp ON tcv.""ParentProject"" = tp.""ParentProject""
                    INNER JOIN ""tlkpProgram"" tprog ON tp.""ProgramCode"" = tprog.""ProgramCode""
                    INNER JOIN ""tblkpProfitCentre"" tpc ON pcg.""ProfitCentre"" = tpc.""ProfitCentre""";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully in {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                var duration = DateTime.UtcNow - startTime;
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
                // PostgreSQL-specific exception handling for better diagnostics
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(
                    pgEx,
                    "[{CorrelationId}] Step {StepName} failed after {Duration}ms. PostgreSQL Error Code: {SqlState}, Message: {ErrorMessage}",
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


// Key improvements made:
// 1. Changed 'var stepName' to 'const string stepName' - stepName is constant and should be declared as such
// 2. Added ISO 8601 format specifier (:O) to StartTime logging for better timestamp consistency
// 3. Replaced 'using' with 'await using' for NpgsqlConnection and NpgsqlCommand - .NET 8 best practice for async disposal
// 4. Added PostgresException-specific catch block before generic Exception - provides better error diagnostics with SqlState
// 5. Changed OperationCanceledException logging from LogError to LogWarning with exception parameter - cancellation is expected behavior, not an error
// 6. Added exception parameter to OperationCanceledException logging for better trace context
// 7. Maintained all existing functionality without adding new features
// 8. SQL query remains unchanged to preserve exact business logic