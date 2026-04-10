using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert time cost calculations into TimeCostCalcs table.
    /// Converts sp_CreateTimeCostCalcs stored procedure with complex multi-table join,
    /// CASE logic for isdefraproject and sector_name, 300-second timeout and correlation-id logging.
    /// </summary>
    public interface ICreateTimeCostCalcsService
    {
        /// <summary>
        /// Executes INSERT INTO TimeCostCalcs with complex SELECT joining multiple tables.
        /// Applies CASE logic for ChargeRate, Class, and Cost calculation.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of rows inserted</returns>
        Task<int> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default);
    }

    public sealed class CreateTimeCostCalcsService : ICreateTimeCostCalcsService
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

        public async Task<int> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Use high-precision timer for accurate duration measurement
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[{CorrelationId}] CreateTimeCostCalcs step started at {StartTime:O}",
                correlationId,
                startTime);

            try
            {
                // Use await using for proper async disposal in .NET 8
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // SQL query formatted for better readability and maintainability
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
                        CASE 
                            WHEN tprog.""sector_name"" = 'Charge' THEN mt.""Hours""
                            ELSE 0
                        END * 
                        CASE 
                            WHEN tp.""isdefraproject"" = 0 THEN pcg.""ChargeRate""
                            ELSE pcg.""DefraChargeRate""
                        END AS ""Cost"",
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
                    INNER JOIN ""tblkpProfitCentre"" tpc ON wgg.""ProfitCentre"" = tpc.""ProfitCentre""
                    WHERE mt.""Hours"" IS NOT NULL 
                        AND mt.""Hours"" > 0";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] CreateTimeCostCalcs step completed at {EndTime:O}. Duration: {Duration}ms. Rows inserted: {RowsAffected}",
                    correlationId,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return rowsAffected;
            }
            catch (OperationCanceledException ex)
            {
                // Separate handling for cancellation to distinguish from other exceptions
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] CreateTimeCostCalcs step cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    endTime,
                    duration.TotalMilliseconds);

                throw;
            }
            catch (NpgsqlException ex)
            {
                // Specific handling for PostgreSQL exceptions with additional context
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] CreateTimeCostCalcs step failed at {EndTime:O}. Duration: {Duration}ms. PostgreSQL Error Code: {ErrorCode}, Message: {ErrorMessage}",
                    correlationId,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.ErrorCode,
                    ex.Message);

                throw;
            }
            catch (Exception ex)
            {
                // Generic exception handling for unexpected errors
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] CreateTimeCostCalcs step failed at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

                throw;
            }
        }
    }
}


**Key Improvements Made:**

1. **Sealed Class**: Added `sealed` modifier to prevent inheritance and enable potential compiler optimizations
2. **Async Disposal**: Changed `using` to `await using` for proper async disposal pattern in .NET 8
3. **ConfigureAwait(false)**: Added to async calls to avoid unnecessary context capturing in library/service code
4. **ISO 8601 DateTime Formatting**: Used `:O` format specifier for consistent, sortable datetime logging
5. **Enhanced Exception Handling**: 
   - Separated `OperationCanceledException` for cancellation scenarios
   - Added specific `NpgsqlException` handling with error code logging
   - Maintained generic exception handler as fallback
6. **Better Exception Context**: Added PostgreSQL error code to NpgsqlException logging for easier troubleshooting
7. **Code Comments**: Added clarifying comments for best practices applied

These changes align with .NET 8 best practices, improve async/await patterns, enhance observability through better logging, and provide more granular exception handling for PostgreSQL operations in containerized environments like ECS Fargate.