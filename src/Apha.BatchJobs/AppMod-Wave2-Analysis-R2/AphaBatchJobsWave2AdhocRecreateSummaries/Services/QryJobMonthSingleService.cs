using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to insert single month project data into ProjectMonth2 table.
    /// Converts sp_qryJobMonth_Single stored procedure with 300-second timeout and correlation-id logging.
    /// </summary>
    public class QryJobMonthSingleService
    {
        private readonly string _connectionString;
        private readonly ILogger<QryJobMonthSingleService> _logger;
        private const int CommandTimeoutSeconds = 300;

        public QryJobMonthSingleService(
            string connectionString,
            ILogger<QryJobMonthSingleService> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes INSERT INTO ProjectMonth2 with SELECT query aggregating project month data.
        /// Uses parameterized PostgreSQL command with 300-second timeout.
        /// Logs step start, end, duration with correlation id.
        /// Returns success/failure status.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for tracking execution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken = default)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty", nameof(correlationId));
            }

            const string stepName = "sp_qryJobMonth_Single";
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

                await using var command = connection.CreateCommand();
                command.CommandTimeout = CommandTimeoutSeconds;
                command.CommandType = CommandType.Text;

                // SQL query remains the same but formatted for better readability
                command.CommandText = @"
                    INSERT INTO ""ProjectMonth2"" (
                        ""Project"",
                        ""MonthNo"",
                        ""CostProfile"",
                        ""Subcontracts"",
                        ""Animals"",
                        ""NonAnimal"",
                        ""TimeCosts"",
                        ""TransferCosts"",
                        ""TotalCost"",
                        ""Invoices"",
                        ""COIW"",
                        ""SumOfCostProfile"",
                        ""PortSales"",
                        ""MstoneDue"",
                        ""Due__Done"",
                        ""OnTime"",
                        ""TotalHours"",
                        ""PayCosts""
                    )
                    SELECT DISTINCT
                        pm.""Project"",
                        pm.""MonthNo"",
                        COALESCE(SUM(pcp.""CostProfile""), 0) AS ""CostProfile"",
                        COALESCE(SUM(psc.""Amount""), 0) AS ""Subcontracts"",
                        COALESCE(SUM(CASE WHEN mo.""TestCode"" LIKE 'A%' THEN mo.""Volume"" * tr.""UnitPrice"" ELSE 0 END), 0) AS ""Animals"",
                        COALESCE(SUM(CASE WHEN mo.""TestCode"" NOT LIKE 'A%' THEN mo.""Volume"" * tr.""UnitPrice"" ELSE 0 END), 0) AS ""NonAnimal"",
                        COALESCE(SUM(tcc.""Cost""), 0) AS ""TimeCosts"",
                        COALESCE(SUM(tc.""Amount""), 0) AS ""TransferCosts"",
                        COALESCE(SUM(pcp.""CostProfile""), 0) + 
                        COALESCE(SUM(psc.""Amount""), 0) + 
                        COALESCE(SUM(mo.""Volume"" * tr.""UnitPrice""), 0) + 
                        COALESCE(SUM(tcc.""Cost""), 0) + 
                        COALESCE(SUM(tc.""Amount""), 0) AS ""TotalCost"",
                        COALESCE(SUM(inv.""Amount""), 0) AS ""Invoices"",
                        COALESCE(SUM(coiw.""Amount""), 0) AS ""COIW"",
                        COALESCE(MAX(pcp.""TotalCostProfile""), 0) AS ""SumOfCostProfile"",
                        COALESCE(SUM(ps.""Amount""), 0) AS ""PortSales"",
                        COALESCE(COUNT(DISTINCT ms.""MilestoneID""), 0) AS ""MstoneDue"",
                        COALESCE(COUNT(DISTINCT CASE WHEN ms.""Status"" = 'Done' THEN ms.""MilestoneID"" END), 0) AS ""Due__Done"",
                        COALESCE(COUNT(DISTINCT CASE WHEN ms.""Status"" = 'OnTime' THEN ms.""MilestoneID"" END), 0) AS ""OnTime"",
                        COALESCE(SUM(tcc.""Time""), 0) AS ""TotalHours"",
                        COALESCE(SUM(tcc.""Pay""), 0) AS ""PayCosts""
                    FROM ""ProjectMonth"" pm
                    LEFT JOIN ""ProjectCostProfile"" pcp ON pm.""Project"" = pcp.""Project"" AND pm.""MonthNo"" = pcp.""MonthNo""
                    LEFT JOIN ""Proj_SubContract"" psc ON pm.""Project"" = psc.""Project"" AND pm.""MonthNo"" = psc.""Month""
                    LEFT JOIN ""MonthlyOutput"" mo ON pm.""Project"" = mo.""Buyer"" AND pm.""MonthNo"" = mo.""Month""
                    LEFT JOIN ""tlkpTestReqmt"" tr ON mo.""TestCode"" = tr.""TestCode"" AND mo.""Buyer"" = tr.""projectBuyerCode""
                    LEFT JOIN ""TimeCostCalcs"" tcc ON pm.""Project"" = tcc.""Project"" AND pm.""MonthNo"" = tcc.""Month""
                    LEFT JOIN ""TransferCosts"" tc ON pm.""Project"" = tc.""Project"" AND pm.""MonthNo"" = tc.""Month""
                    LEFT JOIN ""Invoices"" inv ON pm.""Project"" = inv.""Project"" AND pm.""MonthNo"" = inv.""Month""
                    LEFT JOIN ""COIW"" coiw ON pm.""Project"" = coiw.""Project"" AND pm.""MonthNo"" = coiw.""Month""
                    LEFT JOIN ""PortSales"" ps ON pm.""Project"" = ps.""Project"" AND pm.""MonthNo"" = ps.""Month""
                    LEFT JOIN ""Milestones"" ms ON pm.""Project"" = ms.""Project"" AND pm.""MonthNo"" = ms.""MonthNo""
                    GROUP BY pm.""Project"", pm.""MonthNo""
                    ORDER BY pm.""Project"", pm.""MonthNo""";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime:O}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                // Handle cancellation separately for better observability
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogWarning(
                    ex,
                    "[{CorrelationId}] Step {StepName} was cancelled at {EndTime:O}. Duration: {Duration}ms",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds);

                return false;
            }
            catch (NpgsqlException ex)
            {
                // Handle PostgreSQL-specific exceptions separately for better diagnostics
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed with database error at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}. SqlState: {SqlState}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message,
                    ex.SqlState);

                return false;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(
                    ex,
                    "[{CorrelationId}] Step {StepName} failed at {EndTime:O}. Duration: {Duration}ms. Error: {ErrorMessage}",
                    correlationId,
                    stepName,
                    endTime,
                    duration.TotalMilliseconds,
                    ex.Message);

                return false;
            }
        }
    }
}


**Key improvements made:**

1. **Async Disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly support async disposal patterns in .NET 8.

2. **Input Validation**: Added validation for `correlationId` parameter to prevent null/empty values from being processed.

3. **Const for Step Name**: Changed `stepName` to `const` since it's a compile-time constant and doesn't change.

4. **ISO 8601 DateTime Formatting**: Added `:O` format specifier to DateTime logging for standardized ISO 8601 format, which is better for log aggregation and parsing.

5. **Enhanced Exception Handling**: 
   - Added separate catch block for `OperationCanceledException` to distinguish cancellation from errors
   - Added separate catch block for `NpgsqlException` to capture PostgreSQL-specific error details (SqlState)
   - This provides better observability and debugging capabilities in ECS Fargate environments

6. **Logging Level Adjustment**: Changed cancellation logging to `LogWarning` instead of `LogError` since cancellation is often intentional and not necessarily an error condition.

These changes align with .NET 8 best practices, improve observability in containerized environments (ECS Fargate), and provide better error diagnostics for PostgreSQL operations.