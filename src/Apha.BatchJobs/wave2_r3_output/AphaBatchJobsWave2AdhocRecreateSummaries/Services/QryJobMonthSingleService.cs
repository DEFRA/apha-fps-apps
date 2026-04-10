using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AphaBatchJobsWave2AdhocRecreateSummaries.Services
{
    /// <summary>
    /// Service to execute single month job query inserting aggregated project data into ProjectMonth2 table.
    /// Implements INSERT INTO ProjectMonth2 with aggregated fields from multiple source tables.
    /// Converts sp_qryJobMonth_Single stored procedure logic to PostgreSQL.
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
        /// Executes INSERT INTO ProjectMonth2 with aggregated data from source tables.
        /// Implements the logic from sp_qryJobMonth_Single stored procedure.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for logging and tracking</param>
        /// <param name="cancellationToken">Cancellation token for timeout enforcement</param>
        /// <returns>True if execution succeeded, false otherwise</returns>
        public async Task<bool> ExecuteAsync(string correlationId, CancellationToken cancellationToken)
        {
            const string stepName = "QryJobMonthSingle";
            var startTime = DateTime.UtcNow;
            
            _logger.LogInformation(
                "[{CorrelationId}] Step {StepName} started at {StartTime}",
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
                        COALESCE(SUM(mo.""AnimalCost""), 0) AS ""Animals"",
                        COALESCE(SUM(mo.""NonAnimalCost""), 0) AS ""NonAnimal"",
                        COALESCE(SUM(tcc.""Cost""), 0) AS ""TimeCosts"",
                        COALESCE(SUM(tc.""TransferCost""), 0) AS ""TransferCosts"",
                        COALESCE(SUM(tcc.""Cost""), 0) + 
                        COALESCE(SUM(psc.""Amount""), 0) + 
                        COALESCE(SUM(mo.""AnimalCost""), 0) + 
                        COALESCE(SUM(mo.""NonAnimalCost""), 0) + 
                        COALESCE(SUM(tc.""TransferCost""), 0) AS ""TotalCost"",
                        COALESCE(SUM(inv.""InvoiceAmount""), 0) AS ""Invoices"",
                        COALESCE(SUM(coiw.""COIWAmount""), 0) AS ""COIW"",
                        COALESCE(MAX(pcp.""TotalCostProfile""), 0) AS ""SumOfCostProfile"",
                        COALESCE(SUM(ps.""PortSalesAmount""), 0) AS ""PortSales"",
                        COALESCE(SUM(ms.""MilestoneDue""), 0) AS ""MstoneDue"",
                        COALESCE(SUM(ms.""DueDone""), 0) AS ""Due__Done"",
                        COALESCE(SUM(ms.""OnTime""), 0) AS ""OnTime"",
                        COALESCE(SUM(tcc.""Time""), 0) AS ""TotalHours"",
                        COALESCE(SUM(tcc.""Pay""), 0) AS ""PayCosts""
                    FROM ""ProjectMonth"" pm
                    LEFT JOIN ""ProjectCostProfile"" pcp 
                        ON pm.""Project"" = pcp.""Project"" 
                        AND pm.""MonthNo"" = pcp.""MonthNo""
                    LEFT JOIN ""Proj_SubContract"" psc 
                        ON pm.""Project"" = psc.""Project"" 
                        AND pm.""MonthNo"" = psc.""Month""
                    LEFT JOIN ""MonthlyOutput"" mo 
                        ON pm.""Project"" = mo.""Buyer"" 
                        AND pm.""MonthNo"" = mo.""Month""
                    LEFT JOIN ""TimeCostCalcs"" tcc 
                        ON pm.""Project"" = tcc.""Project"" 
                        AND pm.""MonthNo"" = tcc.""Month""
                    LEFT JOIN ""TransferCosts"" tc 
                        ON pm.""Project"" = tc.""Project"" 
                        AND pm.""MonthNo"" = tc.""Month""
                    LEFT JOIN ""Invoices"" inv 
                        ON pm.""Project"" = inv.""Project"" 
                        AND pm.""MonthNo"" = inv.""Month""
                    LEFT JOIN ""COIW"" coiw 
                        ON pm.""Project"" = coiw.""Project"" 
                        AND pm.""MonthNo"" = coiw.""Month""
                    LEFT JOIN ""PortSales"" ps 
                        ON pm.""Project"" = ps.""Project"" 
                        AND pm.""MonthNo"" = ps.""Month""
                    LEFT JOIN ""Milestones"" ms 
                        ON pm.""Project"" = ms.""Project"" 
                        AND pm.""MonthNo"" = ms.""Month""
                    GROUP BY 
                        pm.""Project"",
                        pm.""MonthNo""";

                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "[{CorrelationId}] Step {StepName} completed successfully at {EndTime}. Duration: {Duration}ms. Rows affected: {RowsAffected}",
                    correlationId,
                    stepName,
                    DateTime.UtcNow,
                    duration.TotalMilliseconds,
                    rowsAffected);

                return true;
            }
            catch (OperationCanceledException ex)
            {
                var duration = DateTime.UtcNow - startTime;
                // Log with exception object for better observability
                _logger.LogWarning(
                    ex,
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


**Key improvements made:**

1. **Async Disposal (.NET 8)**: Changed `using` to `await using` for `NpgsqlConnection` and `NpgsqlCommand` to properly leverage async disposal patterns in .NET 8
2. **Variable Declaration**: Moved `stepName` to `const` and `startTime` declaration closer to usage for better code organization
3. **Exception Handling**: Changed `OperationCanceledException` logging from `LogError` to `LogWarning` since cancellation is an expected flow control mechanism, not an error. Also added the exception object to the log for better observability
4. **Code Consistency**: Maintained all existing functionality without adding new features